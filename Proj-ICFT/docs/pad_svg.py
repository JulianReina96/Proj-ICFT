# Inspeciona e adiciona margem ao topo de um SVG do PlantUML (corrige clipping do Smetana).
import re, sys

inp = sys.argv[1] if len(sys.argv) > 1 else "diagrama-casos-uso.svg"
PAD = 28  # px de margem extra no topo

s = open(inp, encoding="utf-8").read()
tag = re.search(r"<svg[^>]*>", s).group(0)

w = float(re.search(r'width="(\d+(?:\.\d+)?)', tag).group(1))
h = float(re.search(r'height="(\d+(?:\.\d+)?)', tag).group(1))
vb = re.search(r'viewBox="([^"]+)"', tag)
print("ANTES:", "w=", w, "h=", h, "viewBox=", vb.group(1) if vb else None)

# menor coordenada y de conteúdo (ellipses/textos) — detecta o que está acima de 0
cys = [float(x) for x in re.findall(r'cy="(-?\d+(?:\.\d+)?)"', s)]
tys = [float(x) for x in re.findall(r'<text[^>]*\sy="(-?\d+(?:\.\d+)?)"', s)]
miny = min(cys + tys) if (cys or tys) else 0.0
print("min cy:", min(cys) if cys else None, "min text y:", min(tys) if tys else None)
print("tem Registrar-se:", "Registrar-se" in s)

# Novo topo: o que for mais alto entre conteúdo acima de 0 e a margem desejada
top = min(0.0, miny) - PAD
new_h = h + (0.0 - top)

new_tag = tag
new_tag = re.sub(r'height="\d+(?:\.\d+)?(px)?"', f'height="{new_h:.0f}px"', new_tag, count=1)
new_tag = re.sub(r'viewBox="[^"]+"', f'viewBox="0 {top:.0f} {w:.0f} {new_h:.0f}"', new_tag, count=1)
if "viewBox" not in new_tag:
    new_tag = new_tag.replace("<svg", f'<svg viewBox="0 {top:.0f} {w:.0f} {new_h:.0f}"', 1)

out = s.replace(tag, new_tag, 1)
open(inp, "w", encoding="utf-8").write(out)
print("DEPOIS:", "h=", round(new_h), "viewBox top=", round(top))
