/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2019/8/7
 * Title   : 
 * Desc    : 
*************************************************************************************/

namespace FH
{
    public static class PBSerializer
    {
        public static bool Serialize(IPBMessage message, PBWriter writer)
        {
            if (message == null || writer == null)
                return false;
            writer.Begin();
            message.Serialize(writer);
            writer.End();
            return true;
        }

        public static T Unserialize<T>(PBReader reader) where T : class, IPBMessage, new()
        {
            if (reader == null)
                return null;
            T msg = new T();
            if (msg.Unserialize(reader))
                return msg;
            if (reader.HasError())
                return null;
            return null;
        }
    }
}
