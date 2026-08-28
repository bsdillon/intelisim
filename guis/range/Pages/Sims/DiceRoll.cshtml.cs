using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyApp.Namespace
{
    public class DiceRollModel : PageModel
    {
        public void OnGet()
        {
            Console.WriteLine("GO!  DICE ROLL!");
        }
    }
}