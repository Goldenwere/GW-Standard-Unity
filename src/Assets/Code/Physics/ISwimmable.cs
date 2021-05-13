namespace Goldenwere.Unity.PhysicsUtil
{
    /// <summary>
    /// Interface for swimmable bodies
    /// </summary>
    public interface ISwimmable
    {
        /// <summary>
        /// When a BodyOfWater's OnTriggerEnter is called, it will find ISwimmable and send itself with this method
        /// </summary>
        /// <param name="water">The body of water that the ISwimmable has entered</param>
        void OnWaterEnter(BodyOfWater water);

        /// <summary>
        /// When a BodyOfWater's OnTriggerExit is called, it will find ISwimmable and send itself with this method
        /// </summary>
        /// <param name="water">The body of water that the ISwimmable has entered</param>
        void OnWaterExit(BodyOfWater water);
    }
}