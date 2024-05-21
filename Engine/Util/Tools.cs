using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Microsoft.Xna.Framework.Graphics.SpriteFont;

namespace TestShader
{



    delegate void ActionRef<T1, T2>(T1 t1, ref T2 t2);

    /// <summary>
    /// Collection of static methods to create default actions<br/>
    /// </summary>
    public static class Actions
    {
        /// <summary>
        /// Represents an empty action<br/>
        /// </summary>
        public static void Empty() { }

        /// <summary>
        /// Represents an empty action that takes one parameter<br/>
        /// </summary>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="value">The parameter value</param>
        public static void Empty<T>(T value) { }

        /// <summary>
        /// Represents an empty action that takes two parameters<br/>
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter</typeparam>
        /// <typeparam name="T2">Type of the second parameter</typeparam>
        /// <param name="value1">The first parameter value</param>
        /// <param name="value2">The second parameter value</param>
        public static void Empty<T1, T2>(T1 value1, T2 value2) { }

        /// <summary>
        /// Reduces an action to take a single parameter<br/>
        /// </summary>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="action">The action to be reduced</param>
        /// <returns>The reduced action</returns>
        public static Action<T> Reduce<T>(Action action)
        {
            return (t => action());
        }

        /// <summary>
        /// Reduces an action to take two parameters<br/>
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter</typeparam>
        /// <typeparam name="T2">Type of the second parameter</typeparam>
        /// <param name="action">The action to be reduced</param>
        /// <returns>The reduced action</returns>
        public static Action<T1, T2> Reduce<T1, T2>(Action action)
        {
            return ((t1, t2) => action());
        }

        /// <summary>
        /// Determines if the list of invocations of the delegate <paramref name="del"/> contains the delegate <paramref name="meth"/><br/>
        /// Both delegates must have 2 parameters<br/>
        /// </summary>
        /// <typeparam name="T1"> The first parameter type</typeparam>
        /// <typeparam name="T2"> The second parameter type</typeparam>
        /// <param name="del"> The delegate to check for containing <paramref name="meth"/></param>
        /// <param name="meth"> The delegate to look for in the invocations inside <paramref name="del"/></param>
        /// <returns> True if the delegate <paramref name="del"/> contains the delegate <paramref name="meth"/></returns>
        public static bool delHasMethod<T1, T2>(Action<T1, T2> del, Action<T1, T2> meth)
        {
            return del.GetInvocationList().Where((dels) => (dels.Equals(meth))).Any();
        }
    }

    /// <summary>
    /// A collection of static methods to perform various operations on Tags and few functions to define basic delagates by.<br/>
    /// </summary>
    public static class Functions
    {
        /// <summary>
        /// Creates a delegate that checks if a given Tag is a subclass of the specified type T or if it is of type T itself.<br/>
        /// </summary>
        /// <typeparam name="T">The type to check against. Must be a subclass of Tag.</typeparam>
        /// <returns>A delegate that takes a Tag and returns a boolean indicating whether it is a subclass of T or of type T.</returns>
        private static Func<Tag, bool> IsSubtagDelegate<T>() where T : Tag
        {
            return (attribute) => (attribute.GetType().IsSubclassOf(typeof(T)) || attribute.GetType() == typeof(T));
        }

        /// <summary>
        /// Retrieves the tags associated with the specified type by getting its custom attributes.<br/>
        /// Allows for retrieving inherited attributes if 'inheritance' is set to true.<br/>
        /// </summary>
        /// <param name="type">The type to retrieve the tags from.</param>
        /// <param name="inheritance">Specifies whether to retrieve inherited attributes. Default is true.</param>
        /// <returns>A list of tags associated with the specified type.</returns>
        public static List<Tag> GetTags(Type type, bool inheritance = true)
        {
            return type.GetCustomAttributes(inheritance).Where((attribute) => (attribute is Tag && (attribute as Tag) != null)).Select((attribute) => (attribute as Tag)).ToList();
        }

        /// <summary>
        /// Checks if the given type has any tags of type T or any of its subtypes.<br/>
        /// </summary>
        /// <typeparam name="T">The type of tag to check for.</typeparam>
        /// <param name="type">The type to check for tags.</param>
        /// <returns>True if the type has any tags of type T or any of its subtypes, false otherwise.</returns>
        public static bool Tag<T>(Type type) where T : Tag
        {
            return GetTags(type, true).Any(IsSubtagDelegate<T>());
        }

        /// <summary>
        /// Retrieves a list of tags of type T from the given type.<br/>
        /// </summary>
        /// <typeparam name="T">The type of tag to retrieve.</typeparam>
        /// <param name="type">The type from which to retrieve the tags.</param>
        /// <returns>A list of tags of type T.</returns>
        public static List<Tag> Tags<T>(Type type) where T : Tag
        {
            return GetTags(type, true).Where(IsSubtagDelegate<T>()).ToList();
        }

        /// <summary>
        /// Retrieves the tags associated with the specified method by getting its custom attributes.<br/>
        /// Allows for retrieving inherited attributes if 'inheritance' is set to true.<br/>
        /// </summary>
        /// <param name="type">The method to retrieve the tags from.</param>
        /// <param name="inheritance">Specifies whether to retrieve inherited attributes. Default is true.</param>
        /// <returns>A list of tags associated with the specified method.</returns>
        public static List<Tag> GetTags(MethodInfo type, bool inheritance = true)
        {
            return type.GetCustomAttributes(inheritance).Where((attribute) => (attribute is Tag && (attribute as Tag) != null)).Select((attribute) => (attribute as Tag)).ToList();
        }

        /// <summary>
        /// Checks if the specified method has a tag of type T or any of its subtypes.<br/>
        /// </summary>
        /// <typeparam name="T">The type of tag to check for.</typeparam>
        /// <param name="type">The method to check for tags.</param>
        /// <returns>True if the method has a tag of type T or any of its subtypes, false otherwise.</returns>
        public static bool Tag<T>(MethodInfo type) where T : Tag
        {
            return GetTags(type, true).Any(IsSubtagDelegate<T>());
        }

        /// <summary>
        /// Lists all tags or subtags of type T for a specified method.<br/>
        /// </summary>
        /// <typeparam name="T">The type of tag to check for.</typeparam>
        /// <param name="type">The method to check for tags.</param>
        /// <returns>List of tags or subtags of type T the method has.</returns>
        public static List<Tag> Tags<T>(MethodInfo type) where T : Tag
        {
            return GetTags(type, true).Where(IsSubtagDelegate<T>()).ToList();
        }

        /// <summary>
        /// Retrieves the tags associated with the specified member by getting its custom attributes.<br/>
        /// Allows for retrieving inherited attributes if 'inheritance' is set to true.<br/>
        /// </summary>
        /// <param name="type">The member to retrieve the tags from.</param>
        /// <param name="inheritance">Specifies whether to retrieve inherited attributes. Default is true.</param>
        /// <returns>A list of tags associated with the specified member.</returns>
        public static List<Tag> GetTags(MemberInfo type, bool inheritance = true)
        {
            return type.GetCustomAttributes(inheritance).Where((attribute) => (attribute is Tag && (attribute as Tag) != null)).Select((attribute) => (attribute as Tag)).ToList();
        }

        /// <summary>
        /// A method that checks if the specified member info has a tag of type T or any of its subtypes.<br/>
        /// /// </summary>
        /// <typeparam name="T">The type of tag to check for.</typeparam>
        /// <param name="type">The MemberInfo to check for tags.</param>
        /// <returns>True if the MemberInfo has a tag of type T or any of its subtypes, false otherwise.</returns>
        public static bool Tag<T>(MemberInfo type) where T : Tag
        {
            return GetTags(type, true).Any(IsSubtagDelegate<T>());
        }

        /// <summary>
        /// Retrieves a list of tags of type T from the given member info.<br/>
        /// </summary>
        /// <typeparam name="T">The type of tag to retrieve.</typeparam>
        /// <param name="type">The member info from which to retrieve the tags.</param>
        /// <returns>A list of tags of type T.</returns>
        public static List<Tag> Tags<T>(MemberInfo type) where T : Tag
        {
            return GetTags(type, true).Where(IsSubtagDelegate<T>()).ToList();
        }

        /// <summary>
        /// Retrieves a list of tags associated with the specified property.<br/>
        /// Allows for retrieving inherited attributes if 'inheritance' is set to true.<br/>
        /// </summary>
        /// <param name="type">The property to retrieve the tags from.</param>
        /// <param name="inheritance">Specifies whether to retrieve inherited attributes. Default is true.</param>
        /// <returns>A list of tags associated with the specified property.</returns>
        public static List<Tag> GetTags(PropertyInfo type, bool inheritance = true)
        {
            return type.GetCustomAttributes(inheritance).Where((attribute) => (attribute is Tag && (attribute as Tag) != null)).Select((attribute) => (attribute as Tag)).ToList();
        }

        /// <summary>
        /// Checks if the specified property info has a tag of type T or any of its subtypes.<br/>
        /// </summary>
        /// <typeparam name="T">The type of tag to check for. Must be a subclass of Tag.</typeparam>
        /// <param name="type">The property info to check for tags.</param>
        /// <returns>True if the property info has a tag of type T or any of its subtypes, false otherwise.</returns>
        public static bool Tag<T>(PropertyInfo type) where T : Tag
        {
            return GetTags(type, true).Any(IsSubtagDelegate<T>());
        }

        /// <summary>
        /// Lists all tags or subtags of type T for a specified property.<br/>
        /// </summary>
        /// <typeparam name="T">The type of tag to check for.</typeparam>
        /// <param name="type">The property to retrieve the tags from.</param>
        /// <returns>A list of tags or subtags of type T the property has.</returns>
        public static List<Tag> Tags<T>(PropertyInfo type) where T : Tag
        {
            return GetTags(type, true).Where(IsSubtagDelegate<T>()).ToList();
        }

        /// <summary>
        /// Retrieves the tags associated with the specified field by getting its custom attributes.<br/>
        /// Allows for retrieving inherited attributes if 'inheritance' is set to true.<br/>
        /// </summary>
        /// <param name="type">The field to retrieve the tags from.</param>
        /// <param name="inheritance">Specifies whether to retrieve inherited attributes. Default is true.</param>
        /// <returns>A list of tags associated with the specified field.</returns>
        public static List<Tag> GetTags(FieldInfo type, bool inheritance = true)
        {
            return type.GetCustomAttributes(inheritance).Where((attribute) => (attribute is Tag && (attribute as Tag) != null)).Select((attribute) => (attribute as Tag)).ToList();
        }

        /// <summary>
        /// Checks if the specified field info has a tag of type T or any of its subtypes.<br/>
        /// </summary>
        /// <typeparam name="T">The type of tag to check for. Must be a subclass of Tag.</typeparam>
        /// <param name="type">The field info to check for tags.</param>
        /// <returns>True if the field info has a tag of type T or any of its subtypes, false otherwise.</returns>
        public static bool Tag<T>(FieldInfo type) where T : Tag
        {
            return GetTags(type, true).Any(IsSubtagDelegate<T>());
        }

        /// <summary>
        /// Retrieves a list of tags of type T from the given field info.<br/>
        /// </summary>
        /// <typeparam name="T">The type of tag to retrieve. Must be a subclass of Tag.</typeparam>
        /// <param name="type">The field info from which to retrieve the tags.</param>
        /// <returns>A list of tags of type T.</returns>
        public static List<Tag> Tags<T>(FieldInfo type) where T : Tag
        {
            return GetTags(type, true).Where(IsSubtagDelegate<T>()).ToList();
        }


        /// <summary>
        /// Identity function that returns the input value as is.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the input value.</typeparam>
        /// <param name="value">The input value.</param>
        /// <returns>The input value.</returns>
        public static T Identity<T>(T value) { return value; }

        /// <summary>
        /// Returns the default value of the specified type.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the value to return.</typeparam>
        /// <returns>The default value of the specified type.</returns>
        public static T0 Default<T0>() { return default(T0); }

        /// <summary>
        /// Returns the default value of the specified type, given a parameter.<br/>
        /// </summary>
        /// <typeparam name="T1">The type of the parameter.</typeparam>
        /// <typeparam name="T0">The type of the value to return.</typeparam>
        /// <param name="value1">The parameter of the specified type.</param>
        /// <returns>The default value of the specified type.</returns>
        public static T0 Default<T1, T0>(T1 value1) { return default(T0); }

        /// <summary>
        /// Checks if the input entity is null.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the input entity.</typeparam>
        /// <param name="entity">The input entity.</param>
        /// <returns>True if the input entity is null, false otherwise.</returns>
        public static bool IsNull<T>(T entity) where T : class { return entity == null; }

        /// <summary>
        /// Checks if the input entity is non-null.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the input entity.</typeparam>
        /// <param name="entity">The input entity.</param>
        /// <returns>True if the input entity is non-null, false otherwise.</returns>
        public static bool IsNonNull<T>(T entity) where T : class { return entity != null; }

        /// <summary>
        /// Returns true for any input value.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the input value.</typeparam>
        /// <param name="entity">The input value.</param>
        /// <returns>True.</returns>
        public static bool True<T>(T entity) { return true; }

        /// <summary>
        /// Returns false for any input value.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the input value.</typeparam>
        /// <param name="entity">The input value.</param>
        /// <returns>False.</returns>
        public static bool False<T>(T entity) { return false; }

    }


    static class Tools
    {

        /// <summary>
        /// Wraps the given text to fit within a specified length, using the specified sprite font and separator.<br/>
        /// </summary>
        /// <param name="spriteFont">The sprite font to use for measuring text.</param>
        /// <param name="text">The text to wrap.</param>
        /// <param name="length">The maximum length of each line.</param>
        /// <param name="separator">The character to use as a separator between words. Defaults to a space.</param>
        /// <param name="textSize">The scale factor to apply to the sprite font. Defaults to 1.</param>
        /// <param name="ignore">A list of characters to ignore when wrapping the text. Defaults to null.</param>
        /// <returns>The wrapped text.</returns>
        public static string WrapText(SpriteFont spriteFont, string text, float length, char separator = ' ', float textSize = 1, List<char> ignore = null)
        {

            string[] words = text.Split(separator);
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;
            float spaceWidth = spriteFont.MeasureString(separator.ToString()).X * textSize;

            foreach (string word in words)
            {
                string niceWord = word;
                if (ignore != null)
                    niceWord = new string(word.Where(c => !ignore.Contains(c)).ToArray());

                Vector2 size = spriteFont.MeasureString(niceWord);

                if ((lineWidth + size.X) * textSize < length)
                {
                    sb.Append(word + separator);
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n" + word + separator);
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.Remove(sb.Length - 1, 1).ToString();
        }



        /// <summary>
        /// Splits a tile sheet into a list of rectangles, each representing a tile.<br/>
        /// </summary>
        /// <param name="width">The width of each tile.</param>
        /// <param name="height">The height of each tile.</param>
        /// <param name="xcount">The number of tiles in the x-axis.</param>
        /// <param name="ycount">The number of tiles in the y-axis.</param>
        /// <returns>A list of rectangles representing the tiles in the tile sheet.</returns>
        public static List<Rectangle> SplitTileSheet(int width, int height, int xcount, int ycount)
        {
            List<Rectangle> result = new List<Rectangle>();
            for (int y = 0; y < ycount; y++)
                for (int x = 0; x < xcount; x++)
                {
                    {
                        result.Add(new Rectangle(x * width, y * height, width, height));
                    }
                }
            return result;

        }

        /// <summary>
        /// Splits a tile sheet into a list of rectangles, each representing a tile.<br/>
        /// </summary>
        /// <param name="texture">The texture containing the tile sheet.</param>
        /// <param name="xcount">The number of tiles in the x-axis.</param>
        /// <param name="ycount">The number of tiles in the y-axis.</param>
        /// <returns>A list of rectangles representing the tiles in the tile sheet.</returns>
        public static List<Rectangle> SplitTileSheet(Texture2D texture, int xcount, int ycount)
        {
            return SplitTileSheet(texture.Width / xcount, texture.Height / ycount, xcount, ycount);

        }

        /// <summary>
        /// Transforms a given point from world coordinates to screen coordinates. (NOT IMPLEMENTED YET)<br/>
        /// </summary>
        /// <param name="worldCoor">The point in world coordinates to be transformed.</param>
        /// <returns>The transformed point in screen coordinates.</returns>
        public static Vector2 Transform(Point worldCoor)
        {
            throw new NotImplementedException();
            Vector2 screenCoor = (worldCoor.ToVector2() - Game.I.Cameras[0]._position.ToVector2()) * Game.I.Cameras[0]._scale + Game.I.Cameras[0].destinationRectangle.Location.ToVector2();
            //Vector2 screenCoor = new Vector2(worldCoor.X - game.Cameras[0]._cameraPosition.X, worldCoor.Y - game.Cameras[0]._cameraPosition.Y);
            float x = screenCoor.X * Game.I._canvasScale.X;
            float y = screenCoor.Y * Game.I._canvasScale.Y;

            float angle = 0;
            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            float x2 = x * cos - y * sin;
            float y2 = x * sin + y * cos;

            return new Vector2(x2, y2);
        }

        /// <summary>
        /// Transforms a given screen coordinate to a world coordinate.<br/>
        /// </summary>
        /// <param name="screenCoor">The screen coordinate to be transformed.</param>
        /// <returns>The transformed world coordinate.</returns>
        public static Point InverseTransform(Vector2 screenCoor)
        {
            float x = screenCoor.X / Game.I._canvasScale.X;
            float y = screenCoor.Y / Game.I._canvasScale.Y;

            int cameraID = -1;
            for (int i = 0; i < Game.I.Cameras.Count; i++)
            {
                if (Game.I.Cameras[i].active && Game.I.Cameras[i].destinationRectangle.Contains(x, y))
                {
                    cameraID = i;
                }
            }
            if (cameraID == -1) return new Point(0, 0);
            Camera camera = Game.I.Cameras[cameraID];
            Vector2 canvasPosition = new Vector2((float)x, (float)y);
            //(canvasPosition - camera.destinationRectangle.Location.ToVector2())/camera._cameraScale+camera._cameraPosition.ToVector2();
            //(position-camera._cameraPosition.ToVector2())*camera._cameraScale + camera.destinationRectangle.Location.ToVector2()

            Point worldCoor = ((canvasPosition - camera.destinationRectangle.Location.ToVector2()) / camera._scale + camera._position.ToVector2()).ToPoint();

            return worldCoor;
        }

        /// <summary>
        /// Enumeration to specify the serialization mode.<br/>
        /// </summary>
        public enum SerializeModes { Binary, XML };

        /// <summary>
        /// Serializes an object to a file using the specified mode.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="filepath">The path to the file to serialize to.</param>
        /// <param name="mode">The mode to use for serialization.</param>
        public static void SerializeToFile<T>(T obj, string filepath, SerializeModes mode)
        {
            using (var fileStream = new FileStream(filepath, FileMode.Create))
            {
                //Serialize
                if (mode == SerializeModes.Binary)
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(fileStream, obj);
                }
                else if (mode == SerializeModes.XML)
                {
                    var xs = new XmlSerializer(typeof(T));
                    xs.Serialize(fileStream, obj);
                }
            }
        }

        /// <summary>
        /// Deserializes an object of type T from a file using the specified filepath and serialization mode.<br/>
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filepath">The path to the file to deserialize from.</param>
        /// <param name="mode">The serialization mode to use.</param>
        /// <returns>The deserialized object of type T.</returns>
        public static T DeserializeFromFile<T>(string filepath, SerializeModes mode)
        {
            T data;
            using (var fileStream = File.OpenRead(filepath))
            {

                //Deserialize
                if (mode == SerializeModes.Binary)
                {
                    var bf = new BinaryFormatter();
                    data = (T)bf.Deserialize(fileStream);
                }
                else
                {
                    var xs = new XmlSerializer(typeof(T));
                    data = (T)xs.Deserialize(fileStream);
                }
            }

            return data;
        }

        /// <summary>
        /// Generates a spiral order traversal of a 2D grid starting at the origin.<br/>
        /// </summary>
        /// <param name="columns">The number of columns in the grid.</param>
        /// <param name="rows">The number of rows in the grid.</param>
        /// <returns>A list of points representing the spiral order traversal.</returns>
        public static List<Point> spiralOrder(int columns, int rows)
        {
            List<Point> arr = new List<Point>();

            if (rows == 0 || columns == 0) return arr;
            var rowBegin = 0;
            var rowEnd = rows - 1;
            var columnBegin = 0;
            var columnEnd = columns - 1;

            while (rowBegin <= rowEnd && columnBegin <= columnEnd)
            {
                for (var i = columnBegin; i <= columnEnd; i++)
                {
                    arr.Add(new Point(rowBegin, i));
                }
                rowBegin++;

                for (var i = rowBegin; i <= rowEnd; i++)
                {
                    arr.Add(new Point(i, columnEnd));
                }
                columnEnd--;

                if (rowBegin <= rowEnd)
                {
                    for (var i = columnEnd; i >= columnBegin; i--)
                    {
                        arr.Add(new Point(rowEnd, i));
                    }
                }
                rowEnd--;

                if (columnBegin <= columnEnd)
                {
                    for (var i = rowEnd; i >= rowBegin; i--)
                    {
                        arr.Add(new Point(i, columnBegin));
                    }
                }
                columnBegin++;
            }
            arr.Reverse();
            return arr;
        }



    }
}
