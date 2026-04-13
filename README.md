# FullstackTestCS
Exercício de FullStack para C# com WPF para FrontEnd e Ado.net com MySql + controle de migração com DbUp para BackEnd

## Requisitos
- Instalar pacotes assim como o modelo a seguir:
  - Microsoft.Extensions.Configuration
  - Microsoft.Extensions.Configuration.UserSecrets
  - MySqlConnector
  - dbup-mysql
  ```bash
  dotnet add package Microsoft.Extensions.Configuration
  ```
- Servidor MySQL em funcionamento
- Definir as seguintes variáveis de ambiente(referentes ao seu banco de dados) assim como o modelo a seguir:
  - DbPort
  - DbName
  - DbUser
  - DbPassword
  ```bash
  dotnet user-secrets set "DbUser" "root"
  ```