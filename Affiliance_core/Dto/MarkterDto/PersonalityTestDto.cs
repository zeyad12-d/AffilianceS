namespace Affiliance_core.Dto.MarkterDto
{
    public class PersonalityTestDto
    {
        public List<PersonalityTestAnswerDto> Answers { get; set; } = new();
    }

    public class PersonalityTestAnswerDto
    {
        public int QuestionId { get; set; }
        public int Answer { get; set; }
    }
}
