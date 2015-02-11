using System;

namespace DarkChat
{
    public class Command
    {
        public string Name;
        public string Description;
        public Action<string> Action;

        public Command(string Name, string Description, Action<string> Action)
        {
            this.Name = Name;
            this.Description = Description;
            this.Action = Action;
        }
    }
}
