# LSP 설정 가이드 (Windows)

클로드코드 내장 csharp-lsp 플러그인은 registerCapability 등 세 가지 요청에 응답을 안 해서 무한 대기에 걸림.
CSharpLspAdapter로 그 요청들을 대신 처리해서 씀. 다른 PC에서 이 프로젝트 열 때 아래 과정을 그대로 반복함.

## 0. 전제
- .NET SDK 8.0 이상 설치돼 있어야 함.
- 아래 항목은 프로젝트 폴더가 아니라 PC(유저 계정) 단위로 설치됨. PC 바꾸면 처음부터 다시 함.

## 1. 기존 공식 csharp-lsp 플러그인 제거
```
/plugin uninstall csharp-lsp@claude-plugins-official
```
유저 `~/.claude/settings.json`의 `enabledPlugins`에 `csharp-lsp@claude-plugins-official`이 남아있으면 지움.

## 2. csharp-ls, 어댑터 설치
```
dotnet tool install --global csharp-ls
dotnet tool install --global CSharpLspAdapter
```
`csharp-ls` 설치가 `DotnetToolSettings.xml` 오류로 실패하면 `dotnet tool install --global csharp-ls --version 0.20.0`으로 구버전 설치함.

PATH에 `%USERPROFILE%\.dotnet\tools`가 있어야 함. 없으면 시스템 환경변수에 추가함.

## 3. 어댑터 플러그인 등록
`%USERPROFILE%\.claude\plugins\csharp-ls-adapter\.claude-plugin\plugin.json`
```json
{
  "name": "csharp-ls-adapter",
  "description": "C# language support via csharp-ls-adapter",
  "version": "1.0.0"
}
```

`%USERPROFILE%\.claude\plugins\csharp-ls-adapter\.lsp.json`
```json
{
  "csharp": {
    "command": "csharp-ls-adapter",
    "extensionToLanguage": {
      ".cs": "csharp",
      ".csx": "csharp"
    }
  }
}
```

`%USERPROFILE%\.claude\plugins\.claude-plugin\marketplace.json`
```json
{
  "name": "local-plugins",
  "owner": { "name": "local" },
  "plugins": [
    {
      "name": "csharp-ls-adapter",
      "source": "./csharp-ls-adapter",
      "description": "C# language support via csharp-ls-adapter"
    }
  ]
}
```

## 4. 유저 settings.json에 마켓플레이스·플러그인 등록
`%USERPROFILE%\.claude\settings.json`의 `enabledPlugins`, `extraKnownMarketplaces`에 추가함.
```json
"enabledPlugins": {
  "csharp-ls-adapter@local-plugins": true
},
"extraKnownMarketplaces": {
  "local-plugins": {
    "source": {
      "source": "directory",
      "path": "~/.claude/plugins"
    }
  }
}
```

## 5. 플러그인 설치, 환경변수 설정
클로드코드 안에서:
```
/plugin install csharp-ls-adapter@local-plugins
```

`ENABLE_LSP_TOOL` 유저 환경변수를 `1`로 설정함(시스템 속성 → 환경 변수 → 사용자 변수).

## 6. 재시작 후 확인
클로드코드를 완전히 종료하고 다시 켬. `.cs` 파일 하나로 LSP `documentSymbol`을 호출해서 심볼이 나오면 정상임.
바로 안 나오면 솔루션 인덱싱 중일 수 있음, 잠시 뒤 재시도함.
