using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace AntiQuickPad.Contract;

// Direct compiled dependencies, not a transitive call graph or a runtime reflection sandbox.
internal static class BoundaryCheck
{
    // Presentation marker manifest: top-level types declared in Editor/Presentation/*.cs.
    // Nested types (including lambdas/iterators) inherit their outermost type's marker.
    // Keeping this here avoids namespace churn or edits to renderer implementation files.
    private static readonly HashSet<string> PresentationTypes = new(StringComparer.Ordinal)
    {
        "AntiQuickPad.Editor.IEditorPresentation", "AntiQuickPad.Editor.EditorPresentation",
        "AntiQuickPad.Editor.MarkdownColorizer", "AntiQuickPad.Editor.CodePanelRenderer",
        "AntiQuickPad.Editor.AdjacentGrouping", "AntiQuickPad.Editor.CheckboxRenderer",
        "AntiQuickPad.Editor.IndentGuideRenderer", "AntiQuickPad.Editor.StrikeRenderer",
        "AntiQuickPad.Editor.ChevronMargin"
    };

    // Exact source -> target exceptions only. Empty: no existing violations are waived.
    private static readonly IReadOnlyDictionary<string, string> KnownViolations = new Dictionary<string, string>();
    private static readonly Dictionary<ushort, OpCode> OpCodesByValue = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!)
        .ToDictionary(op => unchecked((ushort)op.Value));

    internal static void Run()
    {
        using var stream = File.OpenRead(typeof(BoundaryCheck).Assembly.Location);
        using var pe = new PEReader(stream);
        var reader = pe.GetMetadataReader();
        var names = new Signatures(reader);
        var violations = new SortedSet<string>(StringComparer.Ordinal);
        var seenPresentation = new HashSet<string>(StringComparer.Ordinal);
        // Only actual property getter MethodDefs are allowed on ViewModels. Setters, constructors,
        // ordinary methods, event accessors and field access are forbidden to shell/presentation types.
        // ICommand getters are allowed; Execute must dispatch through ICommand, not a VM method.
        var getters = reader.TypeDefinitions.SelectMany(h => reader.GetTypeDefinition(h).GetProperties())
            .Select(h => reader.GetPropertyDefinition(h).GetAccessors().Getter).Where(h => !h.IsNil)
            .Select(h => names.Method(h).Member!).ToHashSet(StringComparer.Ordinal);
        var count = 0;
        foreach (var handle in reader.TypeDefinitions)
        {
            var definition = reader.GetTypeDefinition(handle);
            var source = names.Definition(handle);
            var outer = handle;
            while (!reader.GetTypeDefinition(outer).GetDeclaringType().IsNil) outer = reader.GetTypeDefinition(outer).GetDeclaringType();
            var root = names.Definition(outer);
            var presentation = PresentationTypes.Contains(root.Type);
            if (presentation) seenPresentation.Add(root.Type);
            var shell = In(root.Namespace, "AntiQuickPad.Views.Quiet") || In(root.Namespace, "AntiQuickPad.Views.Card");
            var vm = In(root.Namespace, "AntiQuickPad.ViewModels");
            var controller = In(root.Namespace, "AntiQuickPad.Controller");
            var references = new HashSet<Target>();
            void Add(Use use) { foreach (var target in use.Types) references.Add(target); }
            void Token(EntityHandle token)
            {
                if (!token.IsNil) Add(names.Resolve(token));
            }
            Token(definition.BaseType);
            foreach (var iface in definition.GetInterfaceImplementations()) Token(reader.GetInterfaceImplementation(iface).Interface);
            foreach (var field in definition.GetFields()) Add(reader.GetFieldDefinition(field).DecodeSignature(names, (object?)null));
            foreach (var property in definition.GetProperties())
            {
                var signature = reader.GetPropertyDefinition(property).DecodeSignature(names, (object?)null);
                Add(Signatures.MethodTypes(signature));
            }
            foreach (var ev in definition.GetEvents()) Token(reader.GetEventDefinition(ev).Type);
            foreach (var methodHandle in definition.GetMethods())
            {
                var method = reader.GetMethodDefinition(methodHandle);
                Add(Signatures.MethodTypes(method.DecodeSignature(names, (object?)null)));
                if (method.RelativeVirtualAddress == 0) continue;
                var body = pe.GetMethodBody(method.RelativeVirtualAddress);
                if (!body.LocalSignature.IsNil) Add(reader.GetStandaloneSignature(body.LocalSignature).DecodeLocalSignature(names, (object?)null).Aggregate(Use.Empty, Use.Merge));
                foreach (var region in body.ExceptionRegions) if (region.Kind == ExceptionRegionKind.Catch) Token(region.CatchType);
                var il = body.GetILReader();
                while (il.RemainingBytes > 0)
                {
                    ushort value = il.ReadByte();
                    if (value == 0xfe) value = (ushort)(0xfe00 | il.ReadByte());
                    if (!OpCodesByValue.TryGetValue(value, out var op)) throw new BadImageFormatException($"Unknown IL opcode {value:x} in {source.Type}");
                    switch (op.OperandType)
                    {
                        case OperandType.InlineField:
                        case OperandType.InlineMethod:
                        case OperandType.InlineType:
                        case OperandType.InlineTok:
                        case OperandType.InlineSig:
                            Token(MetadataTokens.EntityHandle(il.ReadInt32())); break;
                        case OperandType.InlineSwitch:
                            var length = il.ReadInt32(); il.Offset = checked(il.Offset + length * 4); break;
                        case OperandType.InlineI8:
                        case OperandType.InlineR: il.Offset += 8; break;
                        case OperandType.InlineBrTarget:
                        case OperandType.InlineI:
                        case OperandType.ShortInlineR:
                        case OperandType.InlineString: il.Offset += 4; break;
                        case OperandType.InlineVar: il.Offset += 2; break;
                        case OperandType.ShortInlineBrTarget:
                        case OperandType.ShortInlineI:
                        case OperandType.ShortInlineVar: il.Offset++; break;
                        case OperandType.InlineNone: break;
                        default: throw new BadImageFormatException($"Unsupported IL operand {op.OperandType}");
                    }
                }
            }
            foreach (var target in references)
            {
                var reason = (shell || presentation) && In(target.Namespace, "AntiQuickPad.Core") ? "presentation references Core" :
                    (shell || presentation) && In(target.Namespace, "AntiQuickPad.ViewModels") && target.Member is not null && !getters.Contains(target.Member) ? "presentation calls/accesses VM outside property getters/ICommand" :
                    (controller || vm) && (In(target.Namespace, "AntiQuickPad.Views") || target.Type is "AntiQuickPad.Look" || target.Type.EndsWith(".ShellPalette", StringComparison.Ordinal) || In(target.Namespace, "Avalonia.Media")) ? "function references presentation" :
                    vm && In(target.Namespace, "Avalonia") ? "ViewModel references Avalonia" : null;
                if (reason is null) continue;
                var route = $"{source.Type} -> {target.Member ?? target.Type}";
                if (KnownViolations.TryGetValue(route, out var explanation)) Console.WriteLine($"BOUNDARY KNOWN {route}: {explanation}");
                else violations.Add($"{route} ({reason})");
            }
            count++;
        }
        foreach (var missing in PresentationTypes.Except(seenPresentation)) violations.Add($"presentation marker missing compiled type: {missing}");
        if (violations.Count > 0) throw new InvalidOperationException("BOUNDARY FAIL:\n" + string.Join("\n", violations));
        Console.WriteLine($"BOUNDARY PASS ({count} types checked)");
    }

    private static bool In(string value, string space) => value == space || value.StartsWith(space + ".", StringComparison.Ordinal);
    private sealed record Target(string Namespace, string Type, string? Member = null);
    private sealed record Use(Target? Owner, ImmutableArray<Target> Types)
    {
        internal static Use Empty => new(null, []);
        internal static Use Of(Target target) => new(target, [target]);
        internal static Use Merge(Use a, Use b) => new(a.Owner ?? b.Owner, a.Types.AddRange(b.Types));
    }

    // Decode all component types, including generic arguments, arrays, modifiers and function pointers.
    private sealed class Signatures(MetadataReader reader) : ISignatureTypeProvider<Use, object?>
    {
        internal Target Definition(TypeDefinitionHandle handle)
        {
            var type = reader.GetTypeDefinition(handle);
            var parent = type.GetDeclaringType();
            if (!parent.IsNil) { var outer = Definition(parent); return new(outer.Namespace, outer.Type + "+" + reader.GetString(type.Name)); }
            var ns = reader.GetString(type.Namespace); var name = reader.GetString(type.Name);
            return new(ns, ns.Length == 0 ? name : ns + "." + name);
        }
        private Target Reference(TypeReferenceHandle handle)
        {
            var type = reader.GetTypeReference(handle);
            if (type.ResolutionScope.Kind == HandleKind.TypeReference)
            { var outer = Reference((TypeReferenceHandle)type.ResolutionScope); return new(outer.Namespace, outer.Type + "+" + reader.GetString(type.Name)); }
            var ns = reader.GetString(type.Namespace); var name = reader.GetString(type.Name);
            return new(ns, ns.Length == 0 ? name : ns + "." + name);
        }
        internal Target Method(MethodDefinitionHandle handle)
        {
            var method = reader.GetMethodDefinition(handle); var type = Definition(method.GetDeclaringType());
            return type with { Member = type.Type + "." + reader.GetString(method.Name) };
        }
        private static Use Member(Use parent, string name, Use signature)
        {
            var result = Use.Merge(parent, signature);
            return parent.Owner is { } owner ? result with { Types = result.Types.Add(owner with { Member = owner.Type + "." + name }) } : result;
        }
        internal Use Resolve(EntityHandle handle)
        {
            switch (handle.Kind)
            {
                case HandleKind.TypeDefinition: return Use.Of(Definition((TypeDefinitionHandle)handle));
                case HandleKind.TypeReference: return Use.Of(Reference((TypeReferenceHandle)handle));
                case HandleKind.TypeSpecification: return reader.GetTypeSpecification((TypeSpecificationHandle)handle).DecodeSignature(this, (object?)null);
                case HandleKind.MemberReference:
                    var member = reader.GetMemberReference((MemberReferenceHandle)handle);
                    var signature = member.GetKind() == MemberReferenceKind.Field ? member.DecodeFieldSignature(this, (object?)null) : MethodTypes(member.DecodeMethodSignature(this, (object?)null));
                    return Member(Resolve(member.Parent), reader.GetString(member.Name), signature);
                case HandleKind.MethodDefinition:
                    var method = reader.GetMethodDefinition((MethodDefinitionHandle)handle);
                    return Member(Use.Of(Definition(method.GetDeclaringType())), reader.GetString(method.Name), MethodTypes(method.DecodeSignature(this, (object?)null)));
                case HandleKind.FieldDefinition:
                    var field = reader.GetFieldDefinition((FieldDefinitionHandle)handle);
                    return Member(Use.Of(Definition(field.GetDeclaringType())), reader.GetString(field.Name), field.DecodeSignature(this, (object?)null));
                case HandleKind.MethodSpecification:
                    var spec = reader.GetMethodSpecification((MethodSpecificationHandle)handle);
                    return spec.DecodeSignature(this, (object?)null).Aggregate(Resolve(spec.Method), Use.Merge);
                case HandleKind.StandaloneSignature:
                    return MethodTypes(reader.GetStandaloneSignature((StandaloneSignatureHandle)handle).DecodeMethodSignature(this, (object?)null));
                default: throw new BadImageFormatException($"Unsupported dependency token {handle.Kind}");
            }
        }
        internal static Use MethodTypes(MethodSignature<Use> signature) => signature.ParameterTypes.Aggregate(signature.ReturnType, Use.Merge);
        public Use GetArrayType(Use elementType, ArrayShape shape) => elementType;
        public Use GetByReferenceType(Use elementType) => elementType;
        public Use GetFunctionPointerType(MethodSignature<Use> signature) => MethodTypes(signature);
        public Use GetGenericInstantiation(Use genericType, ImmutableArray<Use> typeArguments) => typeArguments.Aggregate(genericType, Use.Merge);
        public Use GetGenericMethodParameter(object? genericContext, int index) => Use.Empty;
        public Use GetGenericTypeParameter(object? genericContext, int index) => Use.Empty;
        public Use GetModifiedType(Use modifier, Use unmodifiedType, bool isRequired) => Use.Merge(unmodifiedType, modifier);
        public Use GetPinnedType(Use elementType) => elementType;
        public Use GetPointerType(Use elementType) => elementType;
        public Use GetPrimitiveType(PrimitiveTypeCode typeCode) => Use.Of(new("System", "System." + typeCode));
        public Use GetSZArrayType(Use elementType) => elementType;
        public Use GetTypeFromDefinition(MetadataReader metadata, TypeDefinitionHandle handle, byte rawTypeKind) => Use.Of(Definition(handle));
        public Use GetTypeFromReference(MetadataReader metadata, TypeReferenceHandle handle, byte rawTypeKind) => Use.Of(Reference(handle));
        public Use GetTypeFromSpecification(MetadataReader metadata, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind) => Resolve(handle);
    }
}
