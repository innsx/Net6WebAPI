namespace Application.Wrappers
{
    public class CustomizedAPIResponse<Ttype>
    {
        public Ttype? Data { get; set; }
        public bool Succeed { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();


        //DEFAULT CONSTRUCTOR  
        public CustomizedAPIResponse()
        {            
        }

        //CONSTRUCTOR WITH A Success response format
        public CustomizedAPIResponse(Ttype data, string message = null!)
        {
            Succeed = true;
            Message = message;
            Data = data;
        }

        //CONSTRUCTOR WITH A fail response format
        public CustomizedAPIResponse(string message)
        {
            Succeed = false;
            Message = message;
        }
    }
}