병원 관리 앱(C#, WPF)
<img width="1068" height="589" alt="image" src="https://github.com/user-attachments/assets/3b65b955-d040-4711-a734-4bddab3a15a6" />

<img width="1917" height="1016" alt="image" src="https://github.com/user-attachments/assets/487e0da8-8b11-4086-8c37-653da4166726" />



<img width="1073" height="576" alt="image" src="https://github.com/user-attachments/assets/9cf288aa-e149-43f2-b86e-668a524956bd" />

<img width="1063" height="589" alt="image" src="https://github.com/user-attachments/assets/991d776e-5eb6-4885-9332-bcbfed70defa" />


<img width="882" height="678" alt="image" src="https://github.com/user-attachments/assets/bc73ef75-cbaa-41b5-a6d2-83a5668b427b" />

Page, Frame 이용 Navigation 구현

학생의 대한 날짜별 점수 데이터를  view에서 oxyplot 라이브러리로 만든 그래프

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
