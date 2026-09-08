/* 관계도 HTML 검증기 — 사용법: node Docs/tools/mapcheck.js Docs/SystemMap.html
   html의 <script>만 꺼내 가짜 document로 돌린 뒤, 만들어진 rect(박스)와 path(선) 좌표를 가로채
   관통·겹침·교차를 센다. 브라우저 없이 숫자로 확인하려고 쓴다. */
"use strict";

var fs = require("fs");
var file = process.argv[2] || "Docs/SystemMap.html";
var body = fs.readFileSync(file, "utf8").match(/<script>([\s\S]*)<\/script>/)[1];

var made = [], keyHandlers = [];
function stub() {
  return {
    attrs: {}, tag: "", textContent: "", innerHTML: "",
    setAttribute: function (k, v) { this.attrs[k] = v; },
    appendChild: function () {}, addEventListener: function () {},
    classList: { toggle: function () {} }
  };
}
global.document = {
  getElementById: function () { return stub(); },
  createElementNS: function (ns, tag) { var e = stub(); e.tag = tag; made.push(e); return e; },
  addEventListener: function (name, fn) { if (name === "keydown") keyHandlers.push(fn); }
};

body.split("\n").forEach(function () {});
eval(body);
keyHandlers.forEach(function (fn) { fn({ key: "Escape" }); });   // 전체 보기 상태에서 센다

var boxes = made.filter(function (e) { return e.tag === "rect" && e.attrs.width; })
  .map(function (e) {
    return { x: +e.attrs.x, y: +e.attrs.y, w: +e.attrs.width, h: +e.attrs.height };
  });

function segsOf(d) {
  var pts = d.trim().split(/[ML]\s*/).filter(Boolean)
    .map(function (t) { return t.trim().split(/\s+/).map(Number); });
  var out = [];
  for (var i = 1; i < pts.length; i++) out.push([pts[i - 1], pts[i]]);
  return out;
}
// class에 edge가 붙은 것만 센다. 화살촉(marker) 안의 path는 선이 아니다.
var lines = made.filter(function (e) { return e.tag === "path" && /edge/.test(e.attrs.class || ""); })
  .map(function (e) { return { di: /dieidle/.test(e.attrs.class || ""), segs: segsOf(e.attrs.d) }; });

// 1) 관통 — 선분이 박스 속을 지나가나
var through = 0, samples = [];
lines.forEach(function (l) {
  l.segs.forEach(function (s) {
    var x0 = Math.min(s[0][0], s[1][0]), x1 = Math.max(s[0][0], s[1][0]);
    var y0 = Math.min(s[0][1], s[1][1]), y1 = Math.max(s[0][1], s[1][1]);
    boxes.forEach(function (b) {
      if (x0 < b.x + b.w - 0.5 && x1 > b.x + 0.5 && y0 < b.y + b.h - 0.5 && y1 > b.y + 0.5) {
        through++;
        if (samples.length < 5) samples.push("  " + s[0] + " → " + s[1] + " / 박스 " + b.x + "," + b.y);
      }
    });
  });
});

function horiz(s) { return Math.abs(s[0][1] - s[1][1]) < 0.01; }
function span(s, i) { return [Math.min(s[0][i], s[1][i]), Math.max(s[0][i], s[1][i])]; }

// 2) 교차 — 가로 선분과 세로 선분이 X자로 만남
function crossings(filter) {
  var c = 0;
  for (var i = 0; i < lines.length; i++) {
    if (!filter(lines[i])) continue;
    for (var j = i + 1; j < lines.length; j++) {
      if (!filter(lines[j])) continue;
      lines[i].segs.forEach(function (a) {
        lines[j].segs.forEach(function (b) {
          if (horiz(a) === horiz(b)) return;
          var h = horiz(a) ? a : b, v = horiz(a) ? b : a;
          var hx = span(h, 0), vy = span(v, 1);
          if (v[0][0] > hx[0] + 0.01 && v[0][0] < hx[1] - 0.01 &&
              h[0][1] > vy[0] + 0.01 && h[0][1] < vy[1] - 0.01) c++;
        });
      });
    }
  }
  return c;
}

// 3) 겹침 — 같은 축·같은 좌표에서 구간이 포개짐
var overlaps = 0, ovSamples = [];
for (var i = 0; i < lines.length; i++) {
  for (var j = i + 1; j < lines.length; j++) {
    lines[i].segs.forEach(function (a) {
      lines[j].segs.forEach(function (b) {
        if (horiz(a) !== horiz(b)) return;
        var ax = horiz(a) ? 1 : 0, av = span(a, 1 - ax), bv = span(b, 1 - ax);
        if (Math.abs(a[0][ax] - b[0][ax]) > 0.6) return;
        if (av[0] < bv[1] - 1 && av[1] > bv[0] + 1) { overlaps++;
          if (ovSamples.length < 6) ovSamples.push('  ' + a[0] + '-' + a[1] + ' vs ' + b[0] + '-' + b[1] + '  di:' + lines[i].di + '/' + lines[j].di); }
      });
    });
  }
}

console.log(file);
console.log("  박스 " + boxes.length + " · 선 " + lines.length);
console.log("  관통 " + through + (through ? "  ← 0이어야 한다" : ""));
samples.forEach(function (s) { console.log(s); });
console.log("  겹침 " + overlaps);
ovSamples.forEach(function (t) { console.log(t); });
console.log("  교차 " + crossings(function (l) { return !l.di; }) + " (실선끼리) / " +
            crossings(function () { return true; }) + " (전부)");
process.exit(through ? 1 : 0);
