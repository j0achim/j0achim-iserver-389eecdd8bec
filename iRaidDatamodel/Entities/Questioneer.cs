using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRaidDatamodel.Entities
{
    [LoadInfo]
    public class QuestioneerPartake : DomainLogic<QuestioneerPartake>
    {
        public long UserId { get; private set; }
        public long QuestioneerId { get; private set; }
        public DateTime Time { get; set; }
    }

    [LoadInfo]
    public class Questioneer : DomainLogic<Questioneer>
    {
        public Questioneer(Community community, string title, string description)
        {
            this.CommunityId = community.Id;
            this.Title = title;
            this.Description = description;
            this.Deleted = false;
        }

        public long CommunityId { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }

        /// <summary>
        /// Adds a new question to the questioneer.
        /// </summary>
        /// <param name="Question"></param>
        public void AddQuestion(string Question, User user)
        {
            Question question = new Question(this, Question);
            question.Save(user);
        }

        /// <summary>
        /// Gets list of questions attached to this questioneer.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Question> GetQuestions()
        {
            return Repository<Question>.FindBy(x => x.QuestioneerId == this.Id);
        }
    }

    [LoadInfo]
    public class Question : DomainLogic<Question>
    {
        public Question(Questioneer questioneer, string Question)
        {
            this.QuestioneerId = questioneer.Id;
            this.Quest = Question;
        }

        public long QuestioneerId { get; private set; }
        public string Quest { get; set; }

        /// <summary>
        /// Adds a question-answer to the question.
        /// </summary>
        /// <param name="Answer"></param>
        /// <param name="Correct"></param>
        public void AddAnswer(string Answer, bool Correct, User user)
        {
            QuestionAnswer qa = new QuestionAnswer(this, Answer, Correct);
            qa.Save(user);
        }

        /// <summary>
        /// Gets list of Answers attached to this question.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<QuestionAnswer> GetAnswers()
        {
            return Repository<QuestionAnswer>.FindBy(x => x.QuestionId == this.Id);
        }
    }

    [LoadInfo]
    public class QuestionAnswer : DomainLogic<QuestionAnswer>
    {
        public QuestionAnswer(Question question, string Answer, bool correct)
        {
            this.QuestionId = question.Id;
            this.Answer = Answer;
            this.Correct = correct;
        }
        
        public long QuestionId { get; private set; }
        public string Answer { get; set; }
        public bool Correct { get; set; }
    }
}
