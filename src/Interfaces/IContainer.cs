using System.Collections.Generic;
using TearsInRain.Entities;

namespace TearsInRain.src.Interfaces {
    public interface IContainer {
        List<Item> Inventory { get; set; }

        // Other field possibilities: KeepsTemperature, VisibleFromFar (clear?), MaxVolume, MaxWeight, CurrentVolume, CurrentWeight, Owner, 
    }
}
