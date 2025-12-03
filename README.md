병원 관리 앱(C#, WPF)

<img width="1107" height="733" alt="image" src="https://github.com/user-attachments/assets/7b88b499-6030-49dc-9ffc-5e4dc24656f4" />

Page, Frame 이용 Navigation 구현

학생의 대한 날짜별 점수 데이터를  view에서 oxyplot 라이브러리로 만든 그래프

<img width="878" height="587" alt="image" src="https://github.com/user-attachments/assets/330c577a-de4c-4a7a-87f7-aef4ef1f8a01" />

상적 입력 화면

<img width="879" height="582" alt="image" src="https://github.com/user-attachments/assets/67eb89da-6e3b-4426-8fe7-382ad3564516" />



성적 조회 화면 (수정 및 삭제 가능) 

<img width="877" height="581" alt="image" src="https://github.com/user-attachments/assets/cda612ae-f9ca-41d0-84e0-a17ef6ace368" />

## 처방 자동 발주 이메일 알림 설정

처방 저장 시 재고가 발주점 이하로 내려가 자동 발주가 생성되면, Gmail로 알림 메일을 전송할 수 있습니다. 안전을 위해 환경 변수로 설정합니다.

Windows PowerShell에서 다음 환경 변수를 사용자 계정에 설정하세요:

```powershell
[System.Environment]::SetEnvironmentVariable('GMAIL_USER', 'your@gmail.com', 'User')
[System.Environment]::SetEnvironmentVariable('GMAIL_APP_PASSWORD', 'your-app-password', 'User')
[System.Environment]::SetEnvironmentVariable('GMAIL_TO', 'notify-to@example.com', 'User') # 선택: 미설정시 GMAIL_USER로 발송
[System.Environment]::SetEnvironmentVariable('GMAIL_SMTP_HOST', 'smtp.gmail.com', 'User') # 선택
[System.Environment]::SetEnvironmentVariable('GMAIL_SMTP_PORT', '587', 'User')            # 선택
```

주의:
- Gmail 계정에 2단계 인증(2FA)을 활성화하고 앱 비밀번호(App Password)를 생성하여 `GMAIL_APP_PASSWORD`로 사용하세요.
- 설정 후 IDE/앱을 재시작해야 환경 변수가 인식됩니다.
- 메일 전송 실패는 업무 흐름을 방해하지 않으며 조용히 무시됩니다.