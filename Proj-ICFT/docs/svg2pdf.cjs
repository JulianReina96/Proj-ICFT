// Converte um SVG em PDF vetorial usando o Chromium headless (puppeteer).
// Uso: node svg2pdf.cjs <entrada.svg> <saida.pdf>
const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer');

(async () => {
  const [, , inSvg, outPdf] = process.argv;
  const svg = fs.readFileSync(inSvg, 'utf8');

  // Dimensões do SVG (width/height em px) para dimensionar a página do PDF.
  const wMatch = svg.match(/width="(\d+(?:\.\d+)?)/);
  const hMatch = svg.match(/height="(\d+(?:\.\d+)?)/);
  const wPx = wMatch ? parseFloat(wMatch[1]) : 1200;
  const hPx = hMatch ? parseFloat(hMatch[1]) : 800;
  const pxToIn = (px) => px / 96;

  const html = `<!doctype html><html><head><meta charset="utf-8">
    <style>*{margin:0;padding:0;box-sizing:border-box}html,body{background:#fff}
    svg{display:block}</style></head>
    <body>${svg}</body></html>`;

  const browser = await puppeteer.launch({ headless: 'new', args: ['--no-sandbox'] });
  const page = await browser.newPage();
  await page.setContent(html, { waitUntil: 'networkidle0' });
  await page.pdf({
    path: outPdf,
    printBackground: true,
    pageRanges: '1',
    width: `${pxToIn(wPx) + 0.04}in`,
    height: `${pxToIn(hPx) + 0.04}in`,
    margin: { top: 0, bottom: 0, left: 0, right: 0 }
  });
  await browser.close();
  console.log('PDF gerado:', path.resolve(outPdf));
})();
