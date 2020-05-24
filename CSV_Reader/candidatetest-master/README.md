# CandidateTest

**Тестовое задание для кандидатов на должность программиста ОРТПР ТН-ВВ**

***Предпочтительные технологии:** .NET, WPF*

Cписок задач:

1. Загрузить файл "Input.csv" и "TypeInfos.json" 
2.  Выполнить для каждой строки из Input.csv соединение с TypeInfos.json по типу.

**Пример:**

Для строки: root.SSN1.ZRP1.IL2.ZD1 указан тип ZD.
должно получиться:

| tag | offset |
| ------ | ------ |
| root.SSN1.ZRP1.IL2.ZD1.Cmd | offset |
| root.SSN1.ZRP1.IL2.ZD1.Time01 | offset |
| root.SSN1.ZRP1.IL2.ZD1.Time02 | offset |
| root.SSN1.ZRP1.IL2.ZD1.Time03 | offset |
| ..... |  |
| root.SSN1.ZRP1.IL2.ZD1.Time11_in | offset |
| root.SSN1.ZRP1.IL2.ZD1.Time12_in | offset |
| root.SSN1.ZRP1.IL2.ZD1.Time13_in | offset |

где *offset* - это смещение от предыдушего на размер тип данных

| tag | offset |
| ------ | ------ |
| root.SSN1.ZRP1.IL2.ZD1.Cmd | 0 |
| root.SSN1.ZRP1.IL2.ZD1.Time01 | 4 |
| root.SSN1.ZRP1.IL2.ZD1.Time02 | 12 |
| root.SSN1.ZRP1.IL2.ZD1.Time03 | 20 |


3.  Сохранить полученые значение в формате XML по шаблону.


```xml
<item Binding="Introduced">
    <node-path>{{ tag }}</node-path>
    <address>{{ offset }}</address>
</item>
```


4. Реализовать графический интерфейс для загрузки *.csv файлов и выбора строк .csv необходимых для конвертации.