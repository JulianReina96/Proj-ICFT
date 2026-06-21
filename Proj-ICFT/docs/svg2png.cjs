// Converte um SVG em PNG (alta resolução) usando Chromium headless (puppeteer).
// Uso: node svg2png.cjs <entrada.svg> <saida.png>
const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer');

(async () => {
  const [, , inSvg, outPng] = process.argv;
  const svg = fs.readFileSync(inSvg, 'utf8');
  const wMatch = svg.match(/width="(\d+(?:\.\d+)?)/);
  const hMatch = svg.match(/height="(\d+(?:\.\d+)?)/);
  const wPx = Math.ceil(wMatch ? parseFloat(wMatch[1]) : 1200);
  const hPx = Math.ceil(hMatch ? parseFloat(hMatch[1]) : 800);

  const html = `<!doctype html><html><head><meta charset="utf-8">
    <style>*{margin:0;padding:0}html,body{background:#fff}svg{display:block}</style></head>
    <body>${svg}</body></html>`;

  const browser = await puppeteer.launch({ headless: 'new', args: ['--no-sandbox'] });
  const page = await browser.newPage();
  await page.setViewport({ width: wPx, height: hPx, deviceScaleFactor: 2 });
  await page.setContent(html, { waitUntil: 'networkidle0' });
  const el = await page.$('svg');
  await el.screenshot({ path: outPng, omitBackground: false });
  await browser.close();
  console.log('PNG gerado:', path.resolve(outPng), `${wPx}x${hPx}@2x`);
})();
