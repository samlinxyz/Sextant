public class EulerAngleUtility
{
    public static float UnsignedToSigned(float unsignedEulerAngle)
    {
        return (unsignedEulerAngle - unsignedEulerAngle > 180f ? 360f : 0f);
    }
}
