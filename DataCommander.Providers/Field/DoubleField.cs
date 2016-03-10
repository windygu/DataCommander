namespace DataCommander.Providers
{
    using System.Globalization;

    public sealed class DoubleField
	{
		public DoubleField( double value )
		{
			this.value = value;
		}

		public double Value => this.value;

        public override string ToString()
		{
			return this.value.ToString( "N16", CultureInfo.InvariantCulture );
		}

		private readonly double value;
	}
}