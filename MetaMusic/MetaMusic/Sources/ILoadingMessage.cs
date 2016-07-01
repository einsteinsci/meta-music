using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaMusic.Sources
{
	public interface ILoadingMessage
	{
		string LoadingText
		{ get; }
	}
}
