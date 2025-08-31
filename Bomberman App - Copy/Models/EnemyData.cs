using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bomberman_App.Models
{
    public class EnemyData
    {
        public string Type { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float? TurningDistance { get; set; }
    }
}
