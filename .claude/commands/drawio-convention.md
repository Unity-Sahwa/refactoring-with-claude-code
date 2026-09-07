---
description: AI가 draw.io(.drawio) 다이어그램을 만들 때 지키는 규칙. 그린 뒤 PNG로 내보내 눈으로 검증한다.
---

# drawio 작성 규칙

> 근거: [drawio-mcp/shared/xml-reference.md](https://github.com/jgraph/drawio-mcp/blob/main/shared/xml-reference.md),
> [draw.io — Generate and validate diagrams with AI](https://www.drawio.com/docs/reference/diagram-generation/)
> 목적: **노드가 겹치지 않고, 사람이 열자마자 읽히는 그림**을 한 번에 뽑는 것.

---

## 0. 절차

1. 무엇을 그릴지 **노드 목록과 엣지 목록**부터 글로 확정한다. 좌표는 이때 생각하지 않는다.
2. 아래 1~5절 규칙대로 XML을 쓴다.
3. XML 파싱 검증 → PNG 내보내기 → **이미지를 직접 열어본다.**
4. 겹침·잘림이 보이면 좌표가 아니라 **격자 칸 배정**을 고친다.

---

## 1. 좌표는 격자 산술로만 정한다

원문: `Do NOT compute x/y coordinates in prose... Use the rigid grid below`
`Do NOT verify, re-check, or adjust coordinates after placing a node.`

- 노드마다 **(열 번호, 행 번호)**만 정하고, 좌표는 아래 식으로 기계적으로 계산한다.
- 배치한 뒤 좌표를 눈대중으로 다시 만지지 않는다. 겹치면 칸 번호를 바꾼다.

```
노드 크기 : w = 240, h = 90   (전부 동일. 예외 금지)
열   x = 60  + col * 280
행   y = 40  + row * 140
```

- 한 줄에 4칸까지. 5개째는 다음 행으로 내린다.
- 칸 하나에는 노드 하나만. 같은 (col,row)를 두 번 쓰면 그게 겹침이다.

---

## 2. 레인(컨테이너)을 쓸 때

원문: `Set parent="containerId" on child cells. Children use relative coordinates within the container.`
`Always add pointerEvents=0;`

- 층을 나눌 땐 `swimlane`을 쓰고, **그 층의 노드는 반드시 레인의 자식(`parent="레인id"`)** 으로 넣는다.
- 자식 좌표는 **레인 기준 상대좌표**다. 절대좌표를 쓰면 그림이 흩어진다.
- 레인 스타일에는 `pointerEvents=0;`을 붙인다.
- 레인 크기: `width = 60 + 4*280 = 1180`, `height = 40 + 행수*140 + 20`.
- 레인끼리는 세로로 20px 띄운다.

---

## 3. 엣지는 손으로 라우팅하지 않는다

원문: `Don't hand-route edges. Just declare source and target.`
`draw.io's edge router ... handle routing and placement; you do not need to do layout math.`

- `source`, `target`만 쓴다. `exitX/entryY` 같은 접속점 지정 금지.
- 중간 경유점(`mxPoint` Array) 금지.
- 스타일은 `edgeStyle=orthogonalEdgeStyle;html=1;endArrow=open;` 로 통일한다.
- 엣지 방향은 **의존하는 쪽 → 의존당하는 쪽** 한 방향으로 고정한다.

---

## 4. 라벨은 짧게

- 노드 라벨: 첫 줄에 이름, 아래 최대 2줄. 3줄 넘기면 노드가 아니라 문서에 적을 내용이다.
- 엣지 라벨: **20자 이내 한 줄.** 엣지 라벨이 길어지면 그게 겹침의 주원인이다.
- 여러 타입을 적고 싶으면 대표 1개만 적고 나머지는 하단 메모 박스로 뺀다.
- 설명이 길면 그림 밖 메모 박스(`rounded=0`) 하나에 모은다.

---

## 5. 파일 형식

원문: `AI systems should not generate compressed content.`

- 압축(Base64) 금지. 항상 평문 XML로 쓴다.
- 색은 draw.io 기본 팔레트만 쓴다 — 레인별로 하나씩 고정.
- `<mxfile><diagram><mxGraphModel><root>` 구조를 유지한다. (여러 페이지를 쓸 수 있게)

---

## 6. 검증 (필수, 생략 금지)

원문: `Use the mxfile.xsd schema to validate the XML structure of generated files.`
`Diagrams may require manual refinement of structure, labelling, or layout.`

XML만 검증하고 끝내지 않는다. **반드시 이미지를 본다.**

```bash
# 1) XML 파싱 + 끊긴 엣지 검사
python - <<'EOF'
import xml.dom.minidom
d = xml.dom.minidom.parse('Docs/SystemMap.drawio')
cells = d.getElementsByTagName('mxCell')
ids = {c.getAttribute('id') for c in cells}
bad = [c.getAttribute('id') for c in cells
       if c.getAttribute('edge') == '1'
       and not {c.getAttribute('source'), c.getAttribute('target')} <= ids]
print('cells:', len(cells), 'dangling:', bad)
EOF

# 2) PNG 내보내기 (draw.io 데스크톱 CLI)
"/c/Program Files/draw.io/draw.io.exe" -x -f png --scale 2 \
  -o Docs/SystemMap.png Docs/SystemMap.drawio
```

- PNG를 열어 **노드 겹침 / 라벨 잘림 / 엣지가 노드를 관통하는지** 셋을 본다.
- 하나라도 있으면 1절 격자 칸을 다시 배정하고 처음부터 다시 뽑는다.
- 사람이 손으로 옮겨야 읽히는 그림은 실패로 본다.
