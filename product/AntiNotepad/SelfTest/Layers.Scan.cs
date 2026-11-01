using System.Reflection;
using System.Reflection.Emit;

namespace AntiNotepad;

internal static partial class SelfTest
{
    private static bool InNamespace(string? actual, string segment) =>
        actual == $"AntiNotepad.{segment}" || actual?.StartsWith($"AntiNotepad.{segment}.", StringComparison.Ordinal) == true;

    private static bool IsAntiNotepadNamespace(string? value) =>
        value == "AntiNotepad" || value?.StartsWith("AntiNotepad.", StringComparison.Ordinal) == true;

    private static Type TopLevelDeclaringType(Type type)
    {
        while (type.DeclaringType is { } parent) type = parent;
        return type;
    }

    private static bool IsPInvokeHolder(Type type)
    {
        if (type.Assembly != typeof(SelfTest).Assembly) return false;
        const BindingFlags declared = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Static | BindingFlags.DeclaredOnly;
        return type.GetMethods(declared).Any(IsPInvokeMethod);
    }

    private static bool IsPInvokeMethod(MethodInfo method) => (method.Attributes & MethodAttributes.PinvokeImpl) != 0;

    private static bool IsRawInteropType(Type type, string rawTypeName) => rawTypeName switch
    {
        "Marshal" => type == typeof(System.Runtime.InteropServices.Marshal),
        "IntPtr" => type.FullName == "System.IntPtr",
        "UIntPtr" => type.FullName == "System.UIntPtr",
        "Win32Properties" => type.Name == "Win32Properties",
        _ => false
    };

    private static void AddType(Type? type, HashSet<Type> found, HashSet<Type>? directReferences = null)
    {
        if (type is null) return;
        var added = found.Add(type);
        var directAdded = directReferences?.Add(type) ?? false;
        if (!added && !directAdded) return;
        if (type.HasElementType) AddType(type.GetElementType(), found, directReferences);
        if (type.IsGenericParameter)
        {
            foreach (var constraint in type.GetGenericParameterConstraints()) AddType(constraint, found, directReferences);
        }
        if (type.IsGenericType)
        {
            AddType(type.GetGenericTypeDefinition(), found, directReferences);
            foreach (var argument in type.GetGenericArguments()) AddType(argument, found, directReferences);
        }
        if (type.IsFunctionPointer)
        {
            AddType(type.GetFunctionPointerReturnType(), found, directReferences);
            foreach (var parameter in type.GetFunctionPointerParameterTypes()) AddType(parameter, found, directReferences);
        }
    }

    private static void ScanMethod(MethodBase method, HashSet<Type> found, List<string> errors,
        Action<MethodBase, OpCode, MethodBase>? inspectCall = null,
        Type? allowedProgramTypeToken = null,
        Action<MethodBase, string>? inspectString = null,
        HashSet<Type>? directReferences = null)
    {
        AddType(method.DeclaringType, found, directReferences);
        foreach (var parameter in method.GetParameters()) AddType(parameter.ParameterType, found, directReferences);
        if (method is MethodInfo info) AddType(info.ReturnType, found, directReferences);
        if (method is MethodInfo methodInfo)
        {
            foreach (var argument in methodInfo.GetGenericArguments())
            {
                AddType(argument, found, directReferences);
                if (argument.IsGenericParameter)
                    foreach (var constraint in argument.GetGenericParameterConstraints()) AddType(constraint, found, directReferences);
            }
        }

        MethodBody? body;
        try { body = method.GetMethodBody(); }
        catch (Exception ex) { errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: {ex.GetType().Name}"); return; }
        if (body?.GetILAsByteArray() is not { } il) return;

        var module = method.Module;
        var typeArguments = method.DeclaringType?.GetGenericArguments();
        var methodArguments = method is MethodInfo genericMethod && genericMethod.IsGenericMethod
            ? genericMethod.GetGenericArguments() : null;
        var opcodes = OpCodeMap.Value;
        var position = 0;
        while (position < il.Length)
        {
            var instructionOffset = position;
            var code = il[position++];
            short value = code == 0xfe && position < il.Length
                ? unchecked((short)(0xfe00 | il[position++]))
                : code;
            if (!opcodes.TryGetValue(value, out var opcode))
            {
                errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: unknown IL opcode at {instructionOffset}");
                break;
            }

            var operandStart = position;
            var tokenKind = opcode.OperandType;
            int operandSize;
            switch (tokenKind)
            {
                case OperandType.InlineNone: operandSize = 0; break;
                case OperandType.ShortInlineBrTarget:
                case OperandType.ShortInlineI:
                case OperandType.ShortInlineVar: operandSize = 1; break;
                case OperandType.InlineVar: operandSize = 2; break;
                case OperandType.InlineI:
                case OperandType.InlineBrTarget:
                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineSig:
                case OperandType.InlineString:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                case OperandType.ShortInlineR: operandSize = 4; break;
                case OperandType.InlineI8:
                case OperandType.InlineR: operandSize = 8; break;
                case OperandType.InlineSwitch:
                    if (!HasBytes(il, position, 4)) { errors.Add($"{method.Name}: truncated switch at {instructionOffset}"); return; }
                    var branches = BitConverter.ToInt32(il, position);
                    if (branches < 0 || branches > (il.Length - position - 4) / 4)
                    { errors.Add($"{method.Name}: invalid switch at {instructionOffset}"); return; }
                    operandSize = 4 + branches * 4;
                    break;
                default:
                    errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: unsupported IL operand {tokenKind}");
                    return;
            }
            if (!HasBytes(il, position, operandSize))
            { errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: truncated IL at {instructionOffset}"); return; }

            if (opcode == OpCodes.Ldstr && tokenKind == OperandType.InlineString && inspectString is not null)
            {
                var token = BitConverter.ToInt32(il, operandStart);
                try { inspectString(method, module.ResolveString(token)); }
                catch (Exception ex) { errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: string token 0x{token:x8} {ex.GetType().Name}"); }
            }

            if (operandSize == 4 && tokenKind is OperandType.InlineType or OperandType.InlineMethod or OperandType.InlineField or OperandType.InlineTok)
            {
                var token = BitConverter.ToInt32(il, operandStart);
                try
                {
                    if (tokenKind == OperandType.InlineType)
                        AddType(module.ResolveType(token, typeArguments, methodArguments), found, directReferences);
                    else if (tokenKind == OperandType.InlineMethod)
                    {
                        if (module.ResolveMethod(token, typeArguments, methodArguments) is { } target)
                        {
                            AddMethodReference(target, found);
                            AddType(target.DeclaringType, found, directReferences);
                            inspectCall?.Invoke(method, opcode, target);
                        }
                    }
                    else if (tokenKind == OperandType.InlineField)
                    {
                        if (module.ResolveField(token, typeArguments, methodArguments) is { } field)
                        {
                            AddType(field.DeclaringType, found);
                            AddType(field.FieldType, found);
                            AddType(field.DeclaringType, found, directReferences);
                            AddType(field.FieldType, found, directReferences);
                        }
                    }
                    else if (module.ResolveMember(token, typeArguments, methodArguments) is { } member)
                    {
                        if (member is Type resolvedType)
                        {
                            if (opcode != OpCodes.Ldtoken || resolvedType != allowedProgramTypeToken)
                            {
                                AddType(resolvedType, found);
                                AddType(resolvedType, found, directReferences);
                            }
                        }
                        else if (member is MethodBase resolvedMethod)
                        {
                            AddMethodReference(resolvedMethod, found);
                            AddType(resolvedMethod.DeclaringType, found, directReferences);
                        }
                        else if (member is FieldInfo resolvedField)
                        {
                            AddType(resolvedField.DeclaringType, found);
                            AddType(resolvedField.FieldType, found);
                            AddType(resolvedField.DeclaringType, found, directReferences);
                            AddType(resolvedField.FieldType, found, directReferences);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{method.DeclaringType?.FullName}.{method.Name}: token 0x{token:x8} {ex.GetType().Name}");
                }
            }
            position += operandSize;
        }
    }

    private static void AddMethodReference(MethodBase method, HashSet<Type> found)
    {
        AddType(method.DeclaringType, found);
        foreach (var parameter in method.GetParameters()) AddType(parameter.ParameterType, found);
        if (method is MethodInfo info)
        {
            AddType(info.ReturnType, found);
            foreach (var argument in info.GetGenericArguments()) AddType(argument, found);
        }
    }

    private static bool HasBytes(byte[] il, int position, int count) =>
        count >= 0 && position >= 0 && position <= il.Length - count;

    private static readonly Lazy<Dictionary<short, OpCode>> OpCodeMap = new(() =>
        typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(OpCode))
            .Select(field => (OpCode)field.GetValue(null)!)
            .ToDictionary(opcode => opcode.Value));
}
