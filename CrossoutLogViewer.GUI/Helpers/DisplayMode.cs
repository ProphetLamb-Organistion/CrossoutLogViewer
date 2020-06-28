using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CrossoutLogView.GUI.Helpers
{
    public enum DisplayMode
    {
        [Display(Name = "Avg. Game")]
        GameAvg,
        [Display(Name = "Avg. Round")]
        RoundAvg,
        [Display(Name = "Total")]
        Total
    }
}
