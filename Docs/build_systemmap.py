# -*- coding: utf-8 -*-
# SystemMap.drawio 생성기.
# 화살표가 노드 위를 지나가지 않도록, 세로 이동은 노드 사이 빈 통로에서만 하게 좌표를 계산한다.
from __future__ import unicode_literals
import io

W, H = 240, 90          # 노드 크기 (전부 동일)
COLGAP = 360            # 열 간격 (노드 240 + 여백 120)
X0 = 60                 # 첫 열 x
LANE_X, LANE_W, LANE_H, LANE_GAP = 40, 1440, 220, 40
LANE_Y0, NODE_DY = 40, 60

LANES = [
    ('lane4', 'L4 표현·연출', '#f8cecc', '#b85450'),
    ('lane3', 'L3 도메인',    '#ffe6cc', '#d79b00'),
    ('lane2', 'L2 입력·모드',  '#dae8fc', '#6c8ebf'),
    ('lane1', 'L1 영속·설정',  '#d5e8d4', '#82b366'),
    ('lane0', 'L0 기반',       '#e1d5e7', '#9673a6'),
]

# id: (레인, 열, 라벨)
NODES = {
    'ui':        (0, 0, 'UISystem&#10;진입점: UIRoot / IWindow'),
    'gimmick':   (0, 1, 'GimmickSystem&#10;진입점: EventData 파생'),
    'audio':     (0, 2, 'AudioSystem&#10;진입점: AudioChannel'),
    'player':    (1, 0, 'PlayerSystem&#10;진입점: ICurrentCharacterProvider&#10;IHealthInfo / HitChannel'),
    'camera':    (1, 1, 'CameraSystem&#10;진입점: ILockOnState&#10;CameraRole'),
    'enemy':     (1, 2, 'EnemySystem&#10;진입점: Enemy / WisuMainRe'),
    'combat':    (1, 3, 'CombatSystem&#10;진입점: IDamageable'),
    'input':     (2, 0, 'InputSystem&#10;진입점: IInputMoveProvider&#10;IInputPressedProvider'),
    'gamestate': (2, 1, 'GameStateSystem&#10;진입점: IGameStateProvider&#10;IGameStateController'),
    'save':      (3, 0, 'SaveSystem&#10;진입점: ISaveService&#10;SaveSlotManager'),
    'settings':  (3, 1, 'SettingsSystem&#10;진입점: ISoundSettings&#10;IMouseSettings / IInputKeySettings'),
    'lang':      (3, 2, 'LanguageSystem&#10;진입점: ILanguageSettings'),
    'di':        (4, 0, 'DISystem&#10;진입점: [Inject] / IDataProvider&#10;씬 Awake 1회 주입'),
}

NAME = {'ui': 'UI', 'gimmick': 'Gimmick', 'audio': 'Audio', 'player': 'Player',
        'camera': 'Camera', 'enemy': 'Enemy', 'combat': 'Combat', 'input': 'Input',
        'gamestate': 'GameState', 'save': 'Save', 'settings': 'Settings', 'lang': 'Language'}

# 출발 시스템별 화살표 색 (한 시스템에서 나가는 선을 눈으로 묶어 따라가게)
COLOR = {'ui': '#b85450', 'gimmick': '#d6b656', 'audio': '#9673a6', 'player': '#d79b00',
         'camera': '#3333ff', 'enemy': '#666666', 'combat': '#009999', 'input': '#6c8ebf',
         'settings': '#82b366', 'save': '#82b366', 'lang': '#82b366'}

EDGES = [
    ('ui', 'gamestate', 'Menu 모드 전환'),
    ('ui', 'input', '메뉴 입력'),
    ('ui', 'player', 'IHealthInfo'),
    ('ui', 'settings', '설정 읽기·쓰기'),
    ('ui', 'audio', '버튼 소리'),
    ('ui', 'lang', 'ILanguageSettings'),
    ('gimmick', 'gamestate', 'Cutscene 모드 전환'),
    ('gimmick', 'camera', '컷씬 카메라'),
    ('gimmick', 'player', '체력·위치 조작'),
    ('gimmick', 'save', '세이브·로드'),
    ('gimmick', 'input', '컷씬 스킵 입력'),
    ('audio', 'settings', 'ISoundSettings'),
    ('player', 'input', '이동 입력'),
    ('player', 'camera', '락온 대상'),
    ('player', 'audio', '발소리·타격음'),
    ('camera', 'player', '현재 캐릭터'),
    ('camera', 'input', '락온 입력'),
    ('camera', 'settings', 'IMouseSettings'),
    ('camera', 'gamestate', '컷씬이면 조작 끔'),
    ('enemy', 'player', '추적 타겟'),
    ('combat', 'player', 'HitChannel 구독'),
    ('input', 'settings', '키 설정'),
    ('input', 'gamestate', '현재 모드 구독'),
    ('settings', 'save', 'ISaveService'),
    ('save', 'player', '저장할 값 읽기'),
    ('lang', 'settings', 'SettingsHolder 상속'),
]


def node_box(nid):
    lane, col, _ = NODES[nid]
    x = LANE_X + X0 + col * COLGAP
    y = LANE_Y0 + lane * (LANE_H + LANE_GAP) + NODE_DY
    return x, y, x + W, y + H


def corridor_x(col_left):
    """col_left 노드의 오른쪽 여백(120px) 안에 난 세로 통로 4개."""
    base = LANE_X + X0 + col_left * COLGAP + W          # 여백 시작
    return [base + 24, base + 48, base + 72, base + 96]


def build():
    out = []
    ap = out.append
    ap('<mxfile host="app.diagrams.net" version="24.0.0">')
    ap('  <diagram id="systemmap" name="SystemMap">')
    ap('    <mxGraphModel dx="1600" dy="1400" grid="1" gridSize="10" page="1" '
       'pageWidth="1654" pageHeight="1600" math="0" shadow="0">')
    ap('      <root>')
    ap('        <mxCell id="0" />')
    ap('        <mxCell id="1" parent="0" />')

    for i, (lid, label, fill, stroke) in enumerate(LANES):
        y = LANE_Y0 + i * (LANE_H + LANE_GAP)
        ap('        <mxCell id="%s" value="%s" style="swimlane;horizontal=0;startSize=30;'
           'fillColor=%s;strokeColor=%s;fontStyle=1;pointerEvents=0;" vertex="1" parent="1">'
           % (lid, label, fill, stroke))
        ap('          <mxGeometry x="%d" y="%d" width="%d" height="%d" as="geometry" />'
           % (LANE_X, y, LANE_W, LANE_H))
        ap('        </mxCell>')

    for nid, (lane, col, label) in sorted(NODES.items()):
        stroke = LANES[lane][3]
        ap('        <mxCell id="%s" value="%s" style="rounded=0;whiteSpace=wrap;html=1;'
           'fillColor=#ffffff;strokeColor=%s;fontStyle=1;" vertex="1" parent="%s">'
           % (nid, label, stroke, LANES[lane][0]))
        ap('          <mxGeometry x="%d" y="%d" width="%d" height="%d" as="geometry" />'
           % (X0 + col * COLGAP, NODE_DY, W, H))
        ap('        </mxCell>')

    # 화살표: 출발 노드의 위/아래 변으로 나가 노드 사이 통로를 타고 도착 노드의 변으로 들어간다.
    used = {}
    for i, (src, dst, _) in enumerate(EDGES):
        sx1, sy1, sx2, sy2 = node_box(src)
        tx1, ty1, tx2, ty2 = node_box(dst)
        down = NODES[src][0] < NODES[dst][0]
        same = NODES[src][0] == NODES[dst][0]

        scol, tcol = NODES[src][1], NODES[dst][1]
        # 세로 통로는 출발 열과 도착 열 사이 여백에 낸다. 같은 열이면 그 열 오른쪽 여백을 쓴다.
        left = min(scol, tcol) if scol != tcol else scol
        lanes_x = corridor_x(left)
        slot = used.get(left, 0)
        vx = lanes_x[slot % len(lanes_x)]
        used[left] = slot + 1

        # 수평 이동 높이도 엣지마다 어긋나게 해서 선이 포개지지 않게 한다.
        ch = 14 + (slot % 4) * 14

        if same:                               # 같은 층: 노드 아래(또는 위) 여백으로 우회
            up = (i % 2 == 1)
            hy = (sy1 - ch) if up else (sy2 + ch)
            e = 0 if up else 1
            style = ('exitX=0.5;exitY=%d;exitDx=0;exitDy=0;entryX=0.5;entryY=%d;entryDx=0;entryDy=0;'
                     % (e, e))
            pts = [(sx1 + W // 2, hy), (tx1 + W // 2, hy)]
        elif down:
            style = 'exitX=0.5;exitY=1;exitDx=0;exitDy=0;entryX=0.5;entryY=0;entryDx=0;entryDy=0;'
            hy1, hy2 = sy2 + ch, ty1 - ch
            pts = [(sx1 + W // 2, hy1), (vx, hy1), (vx, hy2), (tx1 + W // 2, hy2)]
        else:                                  # 아래층 -> 위층
            style = 'exitX=0.5;exitY=0;exitDx=0;exitDy=0;entryX=0.5;entryY=1;entryDx=0;entryDy=0;'
            hy1, hy2 = sy1 - ch, ty2 + ch
            pts = [(sx1 + W // 2, hy1), (vx, hy1), (vx, hy2), (tx1 + W // 2, hy2)]

        color = COLOR.get(src, '#333333')
        ap('        <mxCell id="e%02d" style="edgeStyle=orthogonalEdgeStyle;html=1;rounded=0;'
           '%sstrokeColor=%s;strokeWidth=2.5;endArrow=block;endFill=1;endSize=8;'
           'jumpStyle=arc;jumpSize=10;" edge="1" parent="1" source="%s" target="%s">'
           % (i + 1, style, color, src, dst))
        ap('          <mxGeometry relative="1" as="geometry">')
        ap('            <Array as="points">')
        for px, py in pts:
            ap('              <mxPoint x="%d" y="%d" />' % (px, py))
        ap('            </Array>')
        ap('          </mxGeometry>')
        ap('        </mxCell>')

    # 범례: 그림 위 라벨은 겹침의 주원인이라 전부 빼고 여기 모은다.
    # value는 XML 속성이므로 HTML 태그를 이스케이프해서 넣는다
    rows = ['&lt;b&gt;&lt;font color=&quot;%s&quot;&gt;%s &#8594; %s&lt;/font&gt;&lt;/b&gt; : %s'
            % (COLOR.get(s, '#333333'), NAME[s], NAME[d], t) for s, d, t in EDGES]
    half = (len(rows) + 1) // 2
    note_y = LANE_Y0 + len(LANES) * (LANE_H + LANE_GAP) + 20

    guide = ('읽는 법&#10;'
             '· 화살표 = A가 B를 [Inject]로 받는다. 색은 출발 시스템을 뜻한다.&#10;'
             '· 위층이 아래층을 부르는 게 정상이다. 아래에서 위로 올라가는 화살표는 경계 역전을 의심한다.&#10;'
             '· 먼저 UI → GameState → Player → Camera 한 줄기만 따라간다. 나머지는 곁가지다.&#10;'
             '· 모든 화살표는 DISystem이 씬 Awake 때 한 번에 꽂는다. 런타임 스폰은 주입을 못 받는다.&#10;&#10;'
             '코드에서 확인된 이상 지점&#10;'
             '· ISaveSlots 인터페이스는 UISystem에 있는데 구현은 SaveSystem/SlotLoadRunner.cs다 (경계 역전).&#10;'
             '· GimmickSystem이 SaveSlotManager·SlotLoadRunner를 인터페이스가 아닌 구체 클래스로 받는다.&#10;'
             '· 이 그림은 [Inject] 의존만 그린 것이다. 씬에 직접 붙인 AudioSource·Animator 결합은 안 보인다.')

    boxes = [('guide', LANE_X, 700, guide),
             ('legend1', LANE_X + 740, 350,
              '화살표 범례 (A &#8594; B = A가 B를 [Inject]로 받는다)&#10;&#10;' + '&#10;'.join(rows[:half])),
             ('legend2', LANE_X + 1110, 370, '&#10;&#10;' + '&#10;'.join(rows[half:]))]
    for bid, bx, bw, text in boxes:
        ap('        <mxCell id="%s" value="%s" style="rounded=0;whiteSpace=wrap;html=1;'
           'fillColor=#ffffff;strokeColor=#666666;align=left;verticalAlign=top;fontSize=11;'
           'spacing=8;" vertex="1" parent="1">' % (bid, text))
        ap('          <mxGeometry x="%d" y="%d" width="%d" height="220" as="geometry" />'
           % (bx, note_y, bw))
        ap('        </mxCell>')

    ap('      </root>')
    ap('    </mxGraphModel>')
    ap('  </diagram>')
    ap('</mxfile>')
    return '\n'.join(out) + '\n'


if __name__ == '__main__':
    io.open('SystemMap.drawio', 'w', encoding='utf-8', newline='').write(build())
    print('SystemMap.drawio written: %d nodes, %d edges' % (len(NODES), len(EDGES)))
