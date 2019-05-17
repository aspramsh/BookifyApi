using System.Collections.Generic;

namespace Bookify.Infrastructure.Http
{
    public class ResponseErrorModel
    {
        public ResponseErrorModel()
        {
            ErrorMessages = new List<string>();
        }

        public ResponseErrorModel(params string[] errorMessages)
        {
            ErrorMessages = new List<string>();

            if (errorMessages != null)
            {
                ErrorMessages.AddRange(errorMessages);
            }
        }

        public List<string> ErrorMessages { get; set; }
    }
}
