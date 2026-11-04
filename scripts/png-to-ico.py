from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parents[1]
png_path = root / "src" / "Patcher" / "polus-icon.png"
ico_path = root / "src" / "Patcher" / "polus.ico"

with Image.open(png_path) as source:
    image = source.convert("RGBA")
    if image.size != (512, 512):
        image = image.resize((512, 512), Image.Resampling.LANCZOS)
    image.save(ico_path, format="ICO", sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)])

with Image.open(ico_path) as icon:
    if icon.format != "ICO":
        raise SystemExit("Icon output is not a valid ICO file")
print(f"Created {ico_path}")
