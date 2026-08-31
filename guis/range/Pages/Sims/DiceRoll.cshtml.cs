using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace range
{
    public class DiceRollModel : PageModel
    {
        public void OnGet()
        {
            Console.WriteLine("GO!  DICE ROLL!");
        }
    }
}