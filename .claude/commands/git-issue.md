---
description: GitHub Issue를 템플릿 기반으로 생성한다.
---

## 사용자에게 질문
- 무슨 작업을 할건지

## Issue 문서 만들기
1. `.claude\git\issue`에 파일 생성 `issue_{time}.md ` 
  - time은 현재 년월일시분.
  - ex) issue_202606041521.md
2. `.claude\git\issue\template\{type}.md` 읽기
  - type은 사용자에게 질문했던 Issue type
  
3. 사용자에게 들은 내용과 템플릿 형식으로 생성된 파일 채우기

4. 각 헤더마다 중복되는 내용이 없는지 체크한다.

5. 사용자에게 피드백을 받는다.