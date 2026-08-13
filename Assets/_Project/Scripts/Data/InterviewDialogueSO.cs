using UnityEngine;

[CreateAssetMenu(fileName = "InterviewDialogue", menuName = "TehranCity/Dialogue/Interview Dialogue")]
public class InterviewDialogueSO : ScriptableObject
{
    public string employerName = "کارفرما";

    [TextArea(3, 6)]
    public string employerLine = "قبلاً کار کردی؟ اینجا کار شوخی نیست. یه شیفت آزمایشی، اگه خراب نکنی ادامه میدیم.";

    public string acceptLabel = "قبول می‌کنم";
    public string rejectLabel = "بعداً فکر می‌کنم";

    [TextArea(2, 4)]
    public string acceptedLine = "خوبه. فعلاً از همین امروز شروع کن. صندوق و قفسه رو تمیز نگه دار.";

    [TextArea(2, 4)]
    public string rejectedLine = "باشه. اگه پشیمون شدی، تا آخر هفته وقت داری.";

    public string jobAcceptedFlag = GameFlags.JobAccepted;
}