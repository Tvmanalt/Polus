import fs from "node:fs";
import path from "node:path";
import { Resvg } from "@resvg/resvg-js";

const root = process.cwd();
const input = path.join(root, "assets", "polus-icon.svg");
const output = path.join(root, "src", "Patcher", "polus-icon.png");
const svg = fs.readFileSync(input, "utf8");
const png = new Resvg(svg, { fitTo: { mode: "width", value: 512 } }).render().asPng();
fs.writeFileSync(output, png);
console.log(`Rendered ${input} -> ${output}`);
