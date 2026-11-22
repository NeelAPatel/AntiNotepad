"""List the code points an icon font maps, with glyph names when the font's post table has them.

Usage: python -I A35-ux-pass-s2-fontcodes.py <font.ttf> [hex code ...]
With codes: prints each code, whether the font has it, and its glyph name. Without: prints every PUA code.
"""
import struct, sys


def tables(data):
    count = struct.unpack(">H", data[4:6])[0]
    out = {}
    for i in range(count):
        tag, _, off, length = struct.unpack(">4sIII", data[12 + 16 * i:28 + 16 * i])
        out[tag.decode("latin1")] = (off, length)
    return out


def cmap(data, off):
    n = struct.unpack(">H", data[off + 2:off + 4])[0]
    best = None
    for i in range(n):
        pid, eid, sub = struct.unpack(">HHI", data[off + 4 + 8 * i:off + 12 + 8 * i])
        fmt = struct.unpack(">H", data[off + sub:off + sub + 2])[0]
        if (pid, eid, fmt) in ((3, 10, 12), (0, 4, 12)) or (best is None and fmt == 4):
            best = (off + sub, fmt)
    start, fmt = best
    mapping = {}
    if fmt == 12:
        groups = struct.unpack(">I", data[start + 12:start + 16])[0]
        for g in range(groups):
            s, e, gid = struct.unpack(">III", data[start + 16 + 12 * g:start + 28 + 12 * g])
            for c in range(s, e + 1):
                mapping[c] = gid + c - s
    else:
        segx2 = struct.unpack(">H", data[start + 6:start + 8])[0]
        seg = segx2 // 2
        ends = struct.unpack(f">{seg}H", data[start + 14:start + 14 + segx2])
        starts = struct.unpack(f">{seg}H", data[start + 16 + segx2:start + 16 + 2 * segx2])
        deltas = struct.unpack(f">{seg}h", data[start + 16 + 2 * segx2:start + 16 + 3 * segx2])
        ro_pos = start + 16 + 3 * segx2
        ranges = struct.unpack(f">{seg}H", data[ro_pos:ro_pos + segx2])
        for i in range(seg):
            for c in range(starts[i], ends[i] + 1):
                if c == 0xFFFF:
                    continue
                if ranges[i] == 0:
                    gid = (c + deltas[i]) & 0xFFFF
                else:
                    p = ro_pos + 2 * i + ranges[i] + 2 * (c - starts[i])
                    gid = struct.unpack(">H", data[p:p + 2])[0]
                    gid = (gid + deltas[i]) & 0xFFFF if gid else 0
                if gid:
                    mapping[c] = gid
    return mapping


def post_names(data, off):
    if struct.unpack(">I", data[off:off + 4])[0] != 0x00020000:
        return {}
    n = struct.unpack(">H", data[off + 32:off + 34])[0]
    idx = struct.unpack(f">{n}H", data[off + 34:off + 34 + 2 * n])
    p = off + 34 + 2 * n
    custom = []
    while len(custom) < max([i - 258 for i in idx if i >= 258], default=-1) + 1:
        ln = data[p]
        custom.append(data[p + 1:p + 1 + ln].decode("latin1"))
        p += 1 + ln
    return {gid: custom[i - 258] for gid, i in enumerate(idx) if i >= 258}


def main(path, codes):
    data = open(path, "rb").read()
    t = tables(data)
    m = cmap(data, t["cmap"][0])
    names = post_names(data, t["post"][0]) if "post" in t else {}
    wanted = [int(c, 16) for c in codes] or sorted(c for c in m if 0xE000 <= c <= 0xF8FF)
    for c in wanted:
        gid = m.get(c)
        print(f"{c:04X}\t{'yes' if gid else 'MISSING'}\t{names.get(gid, '') if gid else ''}")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2:])
