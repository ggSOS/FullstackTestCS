-- dialect: mysql

Drop Table If Exists Alunos;

Create Table Alunos
(
    id      BigInt          Not Null    Auto_Increment,
    nome    Varchar(100)    Not Null,
    idade   BigInt          Not Null,
    Primary Key(id)
);