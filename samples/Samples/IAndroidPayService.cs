using System;
using JudoDotNetXamarin;

namespace Samples
{
	public interface IAndroidPayService
	{
		void payment(Judo judo);

		void preAuth(Judo judo);
	}
}
