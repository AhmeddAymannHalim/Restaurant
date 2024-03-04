namespace ResturantPL.Helper
{
    public static class PasswordManager
    {
    
        private static string password = "@Qwe12";

        // Method to create a new password
        public static void CreatePassword(string newPassword)
        {
            password = newPassword;
        }

        // Method to edit the existing password
        public static void EditPassword(string newPassword)
        {
            password = newPassword;
        }

        // Method to validate the entered password
        public static bool ValidatePassword(string enteredPassword)
        {
            return password == enteredPassword;
        }
    }

}
