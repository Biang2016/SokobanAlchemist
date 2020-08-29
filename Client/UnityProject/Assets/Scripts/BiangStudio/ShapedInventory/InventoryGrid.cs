namespace BiangStudio.ShapedInventory
{
    public class InventoryGrid
    {
        public delegate void SetStateDelegate(States newValue);

        public SetStateDelegate SetStateHandler;

        public enum States
        {
            Locked,
            Unavailable,
            TempUnavailable,
            Available,
            Preview
        }

        private States state;

        public States State
        {
            get { return state; }
            set
            {
                if (state != value)
                {
                    SetStateHandler?.Invoke(value);
                    state = value;
                }
            }
        }

        public bool Available => State == States.TempUnavailable || State == States.Available || State == States.Preview;

        public bool Locked => State == States.Locked;
    }
}