
namespace MiscUtil.Core.Compression.Vcdiff
{
	/// <summary>
	/// Contains the information for a single instruction
	/// </summary>
	internal struct Instruction
	{
		readonly InstructionType type;
		internal InstructionType Type
		{
			get { return this.type; }
		}

		readonly byte size;
		internal byte Size
		{
			get { return this.size; }
		}

		readonly byte mode;
		internal byte Mode
		{
			get { return this.mode; }
		}

		internal Instruction(InstructionType type, byte size, byte mode)
		{
			this.type = type;
			this.size = size;
			this.mode = mode;
		}


	}
}

