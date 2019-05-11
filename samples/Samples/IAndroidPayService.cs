using JudoDotNetXamarin;

namespace Samples
{
    public interface IAndroidPayService
    {
        void Payment(Judo judo);

        void PreAuth(Judo judo);
    }
}
