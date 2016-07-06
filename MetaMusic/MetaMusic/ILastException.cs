using System;

namespace MetaMusic
{
	public interface ILastException
	{
		Exception LastException
		{ get; }

		bool HasThrown
		{ get; }

		void ResetException();
	}
}