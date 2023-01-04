using Backend.Controllers;
using Ninject;

namespace Backend
{
    public static class Kernel
    {
        private static IKernel ninjectKernel;

        static Kernel()
        {
            ninjectKernel = new StandardKernel();

            ninjectKernel.Bind<MainController>().ToSelf().InSingletonScope();
            ninjectKernel.Bind<DirectxController>().ToSelf().InSingletonScope();
            ninjectKernel.Bind<VehicleController>().ToSelf().InSingletonScope();
            ninjectKernel.Bind<FileController>().ToSelf().InSingletonScope();
        }

        public static T Get<T>()
        {
            return ninjectKernel.Get<T>();
        }
    }
}
