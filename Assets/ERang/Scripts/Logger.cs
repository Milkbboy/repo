using System.Diagnostics;
using Newtonsoft.Json;

public static class Logger
{
    public static void Log(object logData)
    {
        // 호출된 클래스와 함수 이름을 가져옴
        StackTrace stackTrace = new StackTrace();
        StackFrame frame = stackTrace.GetFrame(1); // 1은 호출한 메서드를 의미

        string className = frame.GetMethod().DeclaringType.Name;
        string methodName = frame.GetMethod().Name;

        // JSON 데이터로 변환
        string logJson = JsonConvert.SerializeObject(logData);

        // 로그 출력
        UnityEngine.Debug.Log($"{className}.{methodName} - {logJson}");
    }
}