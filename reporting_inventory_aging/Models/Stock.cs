namespace reporting_inventory_aging
{
    class Stock
    {
        public string Material { get; set; }
        public decimal Quantity { get; set; }
        public decimal Weight { get; set; }
        public Stock(string _Material, decimal _Quantity, decimal _Weight)
        {
            Material = _Material;
            Quantity = _Quantity;
            Weight = _Weight;
        }

    }
}
