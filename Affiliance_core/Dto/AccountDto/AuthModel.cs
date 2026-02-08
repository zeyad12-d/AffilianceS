namespace Affiliance_core.Dto.AccountDto
{
    public  record AuthModel
    {
        public string Message { get; set; }
        public bool IsAuthenticated { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresOn { get; set; }
       
    }
}
