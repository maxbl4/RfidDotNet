namespace maxbl4.RfidDotNet.AlienTech.Simulator
{
    public class SimulatorOptions
    {
        public string ListenOn { get; set; } = "127.0.0.1:20023";
        public string VisibleTags { get; set; } = "E20000165919004418405CBA,E20000165919006718405C92,E20000165919007818405C7B," +
                                                  "E20000165919007718405C83,E20000165919006518405C91,0307260000000000000090B1," +
                                                  "030726000000000000009213,0307260000000000000092D5,03072600000000000000926D,";

    }
}