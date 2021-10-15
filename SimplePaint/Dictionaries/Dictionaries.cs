using SimplePaint.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePaint.Dictionaries
{
    static class Dictionaries
    {
        public readonly static Dictionary<CommandEnum, string> ActionTypesDictionary = new()
        {
            { CommandEnum.NoAction,"Brak"},
            { CommandEnum.DrawCircle,"Okrąg"},
            { CommandEnum.DrawLine,"Linia"},
            { CommandEnum.DrawReactangle,"Prostokąt"},
            { CommandEnum.Move,"Przesuwanie"},
            { CommandEnum.ReshapeByKeyboard,"Zmiana rozmiaru \n(klawiatura)"},
            { CommandEnum.ReshapeByMouse,"Zmiana rozmiaru \n(mysz)"},
        };
    }
}
