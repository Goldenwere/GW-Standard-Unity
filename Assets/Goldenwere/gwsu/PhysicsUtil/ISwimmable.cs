namespace Goldenwere.Unity.PhysicsUtil
{
    /// <summary>
    /// Interface for swimmable bodies
    /// </summary>
    public interface ISwimmable
    {
        /// <summary>
        /// When a BodyOfFluid's OnTriggerEnter is called, it will find ISwimmable and send itself with this method
        /// </summary>
        /// <param name="fluid">The body of fluid that the ISwimmable has entered</param>
        void OnFluidEnter(BodyOfFluid fluid);

        /// <summary>
        /// When a BodyOfFluid's OnTriggerExit is called, it will find ISwimmable and send itself with this method
        /// </summary>
        /// <param name="fluid">The body of fluid that the ISwimmable has entered</param>
        void OnFluidExit(BodyOfFluid fluid);
    }
}