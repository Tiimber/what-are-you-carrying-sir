using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class Misc {

    public const float DEFAULT_ANIMATION_TIME = 0.3f;

    public static Vector3 VECTOR3_NULL = new Vector3(0, 0, -10000f);

    public static System.Random random = new System.Random();
	public static bool haveScannedInputs = false;
	public static bool hasMouse = true;

	public static byte[] GetBytes(string str) {
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		return bytes;
	}

	public static string GetString(byte[] bytes) {
		char[] chars = new char[bytes.Length / sizeof(char)];
		System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
		return new string(chars);
	}

	private static DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	public static long currentTimeMillis() {
		return (long) ((DateTime.UtcNow - Jan1st1970).TotalMilliseconds);
	}

	public static List<T> CloneBaseNodeList<T>(List<T> list) where T : MonoBehaviour {
		return new List<T>(list);
	}

	public static List<GameObject> NameStartsWith(string start) {
		List<GameObject> matches = new List<GameObject>();
		GameObject[] gameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach (GameObject gameObject in gameObjects) {
			if (gameObject.name.StartsWith(start)) {
				matches.Add(gameObject);
			}
		}
		return matches;
	}

	public static bool isAngleAccepted(float angle1, float angle2, float acceptableAngleDiff, float fullAmountDegrees = 360f) {
		float angleDiff = Mathf.Abs(angle1 - angle2);
		return angleDiff <= acceptableAngleDiff || angleDiff >= fullAmountDegrees - acceptableAngleDiff;
	}

	public static float kmhToMps(float speedChangeKmh) {
		return speedChangeKmh * 1000f / 3600f;
	}

	public static T DeepClone<T>(T original) {
// Construct a temporary memory stream
		MemoryStream stream = new MemoryStream();

// Construct a serialization formatter that does all the hard work
		BinaryFormatter formatter = new BinaryFormatter();

// This line is explained in the "Streaming Contexts" section
		formatter.Context = new StreamingContext(StreamingContextStates.Clone);

// Serialize the object graph into the memory stream
		formatter.Serialize(stream, original);

// Seek back to the start of the memory stream before deserializing
		stream.Position = 0;

// Deserialize the graph into a new set of objects
// and return the root of the graph (deep copy) to the caller
		return (T)(formatter.Deserialize(stream));
	}

	public static Vector3 DivideVectors(Vector3 vec1, Vector3 vec2) {
		return new Vector3(vec1.x / vec2.x, vec1.y / vec2.y, vec1.z / vec2.z);
	}

    public static T pickRandom<T>(List<T> list) {
		if (list.Count > 0) {
			return list[Misc.randomRange(0, list.Count)];
		}
		return default(T);
	}

//	public static string pickRandom(List<string> strings) {
//		return strings[Misc.randomRange(0, strings.Count - 1)];
//	}

    public static V pickRandomValue<K, V>(Dictionary<K, V> dict) {
        if (dict.Count > 0) {
            return dict.ElementAt(Misc.randomRange(0, dict.Count)).Value;
        }
        return default(V);
	}

    public static K pickRandomKey<K, V>(Dictionary<K, V> dict) {
        if (dict.Count > 0) {
            return dict.ElementAt(Misc.randomRange(0, dict.Count)).Key;
        }
        return default(K);
	}

    public static GameObject pickRandomWithWeights(List<int> weights, List<GameObject> gameObjects) {
        int weightSum = weights.Sum();
        int random = Misc.randomRange(0, weightSum);
        int index;
        int accumulatedSum;
        for (index = 0, accumulatedSum = 0; index <= weights.Count; accumulatedSum += weights[index], index++) {
            if (accumulatedSum + weights[index] > random) {
                break;
            }
        }
        return gameObjects[index];
    }

	public static Texture2D MakeTex(int width, int height, Color col) {
		Color[] pix = new Color[width * height];
		for (int i = 0; i < pix.Length; ++i) {
			pix[i] = col;
		}
		Texture2D result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();
		return result;
	}

	public static object getMoney(float money) {
// TODO - Currency for current country - Taken from map info on country
		return "$" + Mathf.Round(money * 100f) / 100f;
	}

	public static object getDistance(float distance) {
// TODO - Maybe in US weird mesurements if in USA
		return Mathf.FloorToInt(distance) + "m";
	}

	public static bool isInside(Vector2 pos, Rect rect) {
		return rect.Contains(pos);
	}

    public static bool isInside(Vector3 pos, Vector3 size, Vector3 parentContainerSize) {
		Bounds objectBounds = new Bounds(pos, size);
        Bounds containerBounds = new Bounds(Vector3.zero, parentContainerSize);
        return containerBounds.Contains(objectBounds.min) && containerBounds.Contains(objectBounds.max);
    }

	public static long daysToTicks(int days) {
		return (long) days * 24 * 60 * 60 * 1000 * 1000 * 10;
	}

	public static float getDistance(Vector3 from, Vector3 to) {
		return (from - to).magnitude;
	}

	public static Vector2 GetLongestDistanceVector(Vector2[] vectors) {
		float longest = float.MinValue;
		Vector2 longestDistance = Vector2.zero;
		for (int i = 1; i < vectors.Length; i++) {
			Vector2 prev = vectors[i - 1];
			Vector2 curr = vectors[i];
			float distance = Misc.getDistance(prev, curr);
			if (distance > longest) {
				longest = distance;
				longestDistance = curr - prev;
			}
		}
		return longestDistance;
	}

	public static Vector2 GetLongestDistanceVector90DegXFrom(Vector2[] vectors, float diffXRotation, float threshold) {
		float longest = float.MinValue;
		Vector2 longestDistance = Vector2.zero;
		for (int i = 1; i < vectors.Length; i++) {
			Vector2 prev = vectors[i - 1];
			Vector2 curr = vectors[i];
			float distance = Misc.getDistance(prev, curr);
			if (distance > longest) {
				Vector2 diffVector = curr - prev;
// float rotation = Misc.ToDegrees(Mathf.Atan(diffVector.y / diffVector.x));
				float rotation = Quaternion.FromToRotation(Vector3.right, diffVector).eulerAngles.z;
				float rotationDiff = Mathf.Abs(rotation - diffXRotation);
				if ((rotationDiff >= 90f - threshold && rotationDiff <= 90f + threshold) || (rotationDiff >= 270f - threshold && rotationDiff <= 270f + threshold)) {
					longest = distance;
					longestDistance = diffVector;
				}
			}
		}
		return longestDistance;
	}

	public static List<GameObject> FindShallowStartsWith(string startsWith) {
		List<GameObject> found = new List<GameObject>();

		GameObject[] gos = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
		foreach (GameObject go in gos) {
			if (go.name.StartsWith(startsWith)) {
				found.Add(go);
			}
		}
		return found;
	}

//Breadth-first search
	public static Transform FindDeepChild(Transform aParent, string aName) {
		var result = aParent.Find(aName);
		if (result != null) {
			return result;
		}
		foreach (Transform child in aParent) {
			result = Misc.FindDeepChild(child, aName);
			if (result != null)
				return result;
		}
		return null;
	}

	public static bool HasParent(GameObject gameObject, Transform parent) {
		Transform transform = gameObject.transform;
		while (transform != null) {
			if (transform == parent) {
				return true;
			}
			transform = transform.parent;
		}

		return false;
	}

	// Convert string with comma separated longs to list of longs
	public static List<long> parseLongs(string longsStr, char separator = ',') {
		List<long> ids = new List<long>();
		if (longsStr != null) {
			string[] idStrings = longsStr.Split(separator);
			foreach (string id in idStrings) {
				ids.Add(Convert.ToInt64(id));
			}
		}
		return ids;
	}

    public static List<float> parseFloats(string floatsStr, char separator = ',') {
        List<float> floats = new List<float>();
        if (floatsStr != null) {
            string[] floatStrings = floatsStr.Split(separator);
            foreach (string floatString in floatStrings) {
                floats.Add(Convert.ToSingle(floatString));
            }
        }
        return floats;
    }

	public static List<List<long>> parseLongMultiList(string intStrings, char listSeparator, char itemSeparator) {
		List<List<long>> multiList = new List<List<long>>();
		string[] lists = intStrings.Split(listSeparator);
		foreach (string list in lists) {
			multiList.Add(Misc.parseLongs(list, itemSeparator));
		}

		return multiList;
	}

	public static string xmlString(XmlNode attributeNode, string defaultValue = null) {
		if (attributeNode != null) {
			return attributeNode.Value;
		}
		return defaultValue;
	}

	public static bool xmlBool(XmlNode attributeNode, bool defaultValue = false) {
		string strVal = Misc.xmlString(attributeNode);
		return strVal == "true" ? true : (strVal == null ? defaultValue : false);
	}

	public static int xmlInt(XmlNode attributeNode, int defaultValue = 0) {
		string strVal = Misc.xmlString(attributeNode);
		if (strVal != null) {
			return Convert.ToInt32(strVal);
		}
		return defaultValue;
	}

	public static float xmlFloat(XmlNode attributeNode, float defaultValue = 0f) {
		string strVal = Misc.xmlString(attributeNode);
		if (strVal != null) {
			return Convert.ToSingle(strVal);
		}
		return defaultValue;
	}

	public static long xmlLong(XmlNode attributeNode, long defaultValue = 0L) {
		string strVal = Misc.xmlString(attributeNode);
		if (strVal != null) {
			return Convert.ToInt64(strVal);
		}
		return defaultValue;
	}

	public static void setRandomSeed(int randomSeed) {
		Misc.random = new System.Random(randomSeed);
	}

	public static float randomPlusMinus(float medium, float plusMinus) {
		return randomRange(medium - plusMinus, medium + plusMinus);
	}

	public static float randomRange(float min, float max) {
		double value = Misc.random.NextDouble();
		return min + (max - min) * (float)value;
	}

	public static int randomRange(int min, int max) {
		return Misc.random.Next(min, max);
	}

	public static bool randomBool() {
		return Misc.random.NextDouble() < 0.5d;
	}

	public static object randomTime() {
		return Misc.randomRange(0, 23) + ":" + Misc.randomRange(0, 59);
	}

	public static DateTime parseDate(string dob) {
		string[] dateParts = dob.Split('-');
		return new DateTime(Convert.ToInt32(dateParts[0]), Convert.ToInt32(dateParts[1]), dateParts.Length > 2 ? Convert.ToInt32(dateParts[2]) : 1);
	}

	public static DateTime parseDateTime(string date, string time) {
		DateTime dateTime = DateTime.Now;
		if (date != null) {
			string[] dateParts = date.Split('-');
			dateTime = dateTime.AddYears(Convert.ToInt32(dateParts[0]) - dateTime.Year);
			dateTime = dateTime.AddMonths(Convert.ToInt32(dateParts[1]) - dateTime.Month);
			dateTime = dateTime.AddDays(Convert.ToInt32(dateParts[2]) - dateTime.Day);
		}
		if (time != null) {
			string[] timeParts = time.Split(':');
			dateTime = dateTime.AddHours(Convert.ToInt32(timeParts[0]) - dateTime.Hour);
			dateTime = dateTime.AddMinutes(Convert.ToInt32(timeParts[1]) - dateTime.Minute);
			dateTime = dateTime.AddSeconds(timeParts.Length > 2 ? Convert.ToInt32(timeParts[2]) - dateTime.Second : -dateTime.Second);
			dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);
		}
		return dateTime;
	}

	public static Color parseColor(string color) {
		string[] colorParts = color.Split(',');
		float r = Convert.ToInt32(colorParts[0]) / 255f;
		float g = Convert.ToInt32(colorParts[1]) / 255f;
		float b = Convert.ToInt32(colorParts[2]) / 255f;

		return new Color(r, g, b);
	}

	public static Vector3 parseVector(string startVector) {
		string[] xy = startVector.Split(',');
		return new Vector3(Convert.ToSingle(xy[0]), Convert.ToSingle(xy[1]), 0);
	}

	public static IEnumerator _WaitForRealSeconds(float aTime) {
		while (aTime > 0f) {
			aTime -= Mathf.Clamp(Time.unscaledDeltaTime, 0, 0.2f);
			yield return null;
		}
	}

	public static Coroutine WaitForRealSeconds(float aTime) {
		Game gameSingleton = Singleton<Game>.Instance;
		return gameSingleton.StartCoroutine(_WaitForRealSeconds(aTime));
	}

	public static Vector3 getWorldPos(Transform transform) {
		Vector3 worldPosition = transform.localPosition;
		while (transform.parent != null) {
			if (transform.parent.localScale != Vector3.one) {
				worldPosition = new Vector3(worldPosition.x * transform.parent.localScale.x, worldPosition.y * transform.parent.localScale.y, worldPosition.z * transform.parent.localScale.z);
			}
			transform = transform.parent;
			worldPosition += transform.localPosition;
		}
		return worldPosition;
	}

	public static Vector3 getWorldPosForParentRelativePos(Vector3 parentRelativePos, Transform parent) {
		Vector3 worldPosition = parentRelativePos;
		while (parent != null) {
			worldPosition += parent.localPosition;
			parent = parent.parent;
		}
		return worldPosition;
	}

	public static Quaternion getWorldRotation(Transform transform) {
		return transform.rotation;
//        Quaternion worldRotation = transform.localRotation;
//        while (transform.parent != null) {
//            transform = transform.parent;
//            worldRotation += transform.localRotation;
//        }
//        return worldRotation;
	}

	public static string maxDecimals(float value, int decimals = 2) {
		if (value != 0f) {
			return value.ToString("#." + getDecimalSpots(decimals));
		} else {
			return value.ToString("0." + getDecimalSpots(decimals));
		}
	}

	private static string getDecimalSpots(int decimals) {
		string decimalChars = "";
		for (int i = 0; i < decimals; i++) {
			decimalChars += "#";
		}
		return decimalChars;
	}

	public class Size {
		public int width;
		public int height;
	}

	public static Size getImageSize(int width, int height, int targetWidth, int targetHeight) {
		Size size = new Size();
		float ratioX = (float) width / targetWidth;
		float ratioY = (float) height / targetHeight;
		if (ratioX > 1f || ratioY > 1f) {
			float scaleFactor = Mathf.Max(ratioX, ratioY);
			size.width = Mathf.RoundToInt(targetWidth / scaleFactor);
			size.height = Mathf.RoundToInt(targetHeight / scaleFactor);
		} else {
			size.width = width;
			size.height = height;
		}
		return size;
	}

	public static AudioListener getAudioListener() {
		AudioListener[] audioListener = Resources.FindObjectsOfTypeAll<AudioListener>();
		if (audioListener != null && audioListener.Length > 0) {
			return audioListener[0];
		}
		return null;
	}

	public static MeshFilter[] FilterCarWays(MeshFilter[] allWayFilters) {
		List<MeshFilter> filtered = new List<MeshFilter>();
		foreach (MeshFilter wayFilter in allWayFilters) {
			if (!(wayFilter.name.StartsWith("CarWay (") || wayFilter.name.StartsWith("NonCarWay ("))) {
				filtered.Add(wayFilter);
			}
		}
		return filtered.ToArray();
	}

	public static float GetHeightRatio() {
		return Mathf.Max((float) Screen.height / (float) Screen.width, 1f);
	}

	public static float GetWidthRatio() {
		return Mathf.Max((float) Screen.width / (float) Screen.height, 1f);
	}

	public static float ToRadians(float degrees) {
		return (Mathf.PI / 180f) * degrees;
	}

	public static float ToDegrees(float radians) {
		return 180f * radians / Mathf.PI;
	}

	public static float getDistanceBetweenEarthCoordinates(float lon1, float lat1, float lon2, float lat2) {
		float R = 6371e3f; // metres
		float φ1 = ToRadians(lat1);
		float φ2 = ToRadians(lat2);
		float Δφ = ToRadians(lat2 - lat1);
		float Δλ = ToRadians(lon2 - lon1);

		float a = Mathf.Sin(Δφ / 2f) * Mathf.Sin(Δφ / 2f) +
		Mathf.Cos(φ1) * Mathf.Cos(φ2) *
		Mathf.Sin(Δλ / 2f) * Mathf.Sin(Δλ / 2f);
		float c = 2f * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1f - a));

		return R * c;
	}

	public static List<int> splitInts(string str) {
		return str.Split(',').Select<string, int>(int.Parse).ToList<int>();
	}

	public static Texture getCountryFlag(string countryCode) {
		return Resources.Load("Graphics/flags/" + countryCode) as Texture;
	}

// Readable in format "1s", "5m 30s" (always number + suffix grouped, parts separated with spaces)
	public static long getTsForReadable(string readable) {
		long ms = 0;
		string[] parts = readable.Split(' ');

		foreach (string part in parts) {
			if (part.EndsWith("ms")) {
				ms += long.Parse(part.Substring(0, part.Length - 2));
			} else if (part.EndsWith("s")) {
				ms += long.Parse(part.Substring(0, part.Length - 1)) * 1000;
			} else if (part.EndsWith("m")) {
				ms += long.Parse(part.Substring(0, part.Length - 1)) * 1000 * 60;
			} else if (part.EndsWith("h")) {
				ms += long.Parse(part.Substring(0, part.Length - 1)) * 1000 * 60 * 60;
			} else if (part.EndsWith("d")) {
				ms += long.Parse(part.Substring(0, part.Length - 1)) * 1000 * 60 * 60 * 24;
			}
		}

		return ms;
	}

	public static float getMeshArea(Mesh mesh) {
		return VolumeOfMesh(mesh);
	}

	private static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3) {
		float v321 = p3.x * p2.y;
		float v231 = p2.x * p3.y;
		float v312 = p3.x * p1.y;
		float v132 = p1.x * p3.y;
		float v213 = p2.x * p1.y;
		float v123 = p1.x * p2.y;
		return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
	}

	private static float VolumeOfMesh(Mesh mesh) {
		float volume = 0;
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
		for (int i = 0; i < mesh.triangles.Length; i += 3) {
			Vector3 p1 = vertices[triangles[i + 0]];
			Vector3 p2 = vertices[triangles[i + 1]];
			Vector3 p3 = vertices[triangles[i + 2]];
			volume += SignedVolumeOfTriangle(p1, p2, p3);
		}
		return Mathf.Abs(volume);
	}

	public static bool CompareVectorLists(List<Vector3> vec1, List<Vector3> vec2) {
		if (vec1 == vec2) {
			return true;
		}

		if (vec1.Count != vec2.Count) {
			return false;
		}

		for (int i = 0; i < vec1.Count; i++) {
			if (!vec1[i].Equals(vec2[i])) {
				return false;
			}
		}

		return true;
	}

	public static Vector2 getScreenPos(Vector3 cameraPos) {
// Vector3 screenPoint = Game.instance.perspectiveCamera.WorldToScreenPoint (cameraPos);
// Revert so that top is 0px
		cameraPos.y = Screen.height - cameraPos.y;
		return cameraPos;
	}

	public class UrlBuilder {
		string url;
		Dictionary<string, string> query = new Dictionary<string, string>();

		public UrlBuilder() {
			this.url = "";
		}

		public UrlBuilder(string url) {
			this.url = url;
		}

		public UrlBuilder addUrl(string url) {
			this.url += url;
			return this;
		}

		public UrlBuilder addQuery(string key, string value) {
			this.query.Add(Uri.EscapeUriString(key), Uri.EscapeUriString(value));
			return this;
		}

		public UrlBuilder addQuery(string key, int value) {
			return addQuery(key, "" + value);
		}

		public UrlBuilder addQuery(string key, float value) {
			return addQuery(key, "" + value);
		}

		public string build() {
			string result = url;
			if (query.Count > 0) {
				result += "?";
				bool first = true;
				foreach (KeyValuePair<string, string> queryPart in query) {
					if (first) {
						first = false;
					} else {
						result += "&";
					}
					result += queryPart.Key + "=" + queryPart.Value;
				}
			}
			return result;
		}
	}

	public static void DestroyChildren(Transform parent) {
		for (int i = parent.childCount - 1; i >= 0; --i) {
			GameObject.Destroy(parent.GetChild(i).gameObject);
		}
		parent.DetachChildren();
	}

	public static Vector3 GetCenterOfVectorList(List<Vector3> vectors) {
		float minX = float.MaxValue;
		float minY = float.MaxValue;
		float maxX = float.MinValue;
		float maxY = float.MinValue;

		foreach (Vector3 vector in vectors) {
			minX = Mathf.Min(vector.x, minX);
			minY = Mathf.Min(vector.y, minY);
			maxX = Mathf.Max(vector.x, maxX);
			maxY = Mathf.Max(vector.y, maxY);
		}

		return new Vector3(minX + (maxX - minX) / 2f, minY + (maxY - minY) / 2f, 0f);
	}

	public static Vector3 GetCenterOfVectorList(Vector2[] vectors) {
		float minX = float.MaxValue;
		float minY = float.MaxValue;
		float maxX = float.MinValue;
		float maxY = float.MinValue;

		foreach (Vector2 vector in vectors) {
			minX = Mathf.Min(vector.x, minX);
			minY = Mathf.Min(vector.y, minY);
			maxX = Mathf.Max(vector.x, maxX);
			maxY = Mathf.Max(vector.y, maxY);
		}

		return new Vector3(minX + (maxX - minX) / 2f, minY + (maxY - minY) / 2f, 0f);
	}

	public static List<Vector3> GetTopMost(List<Vector3> first, List<Vector3> second) {
		Vector3 centerOfFirst = GetCenterOfVectorList(first);
		Vector3 centerOfSecond = GetCenterOfVectorList(second);

		return centerOfFirst.y <= centerOfSecond.y ? first : second;
	}

	public static Rect GetRectOfVectorList(Vector2[] vectors) {
		return GetRectOfVectorList(vectors.ToList());
	}

	public static Rect GetRectOfVectorList(List<Vector3> vectors) {
		return GetRectOfVectorList(vectors);
	}

	public static Rect GetRectOfVectorList(List<Vector2> vectors) {
		float minX = float.MaxValue;
		float minY = float.MaxValue;
		float maxX = float.MinValue;
		float maxY = float.MinValue;

		foreach (Vector2 vector in vectors) {
			minX = Mathf.Min(vector.x, minX);
			minY = Mathf.Min(vector.y, minY);
			maxX = Mathf.Max(vector.x, maxX);
			maxY = Mathf.Max(vector.y, maxY);
		}

		Rect rect = Rect.MinMaxRect(minX, minY, maxX, maxY);
		return rect;
	}

	public static bool IsPointInsideRect(Vector3 point, Rect rect) {
		return point.x >= rect.xMin && point.x <= rect.xMax && point.y >= rect.yMin && point.y <= rect.yMax;
	}

	public static List<Vector3> TieTogetherOuterAndInner(List<Vector3> outerOriginal, List<Vector3> innerOriginal, List<Vector3> outerIntersectionsOriginal, List<Vector3> innerIntersectionsOriginal) {
		try {
			List<Vector3> outer = new List<Vector3>(outerOriginal);
			List<Vector3> inner = new List<Vector3>(innerOriginal);
			List<Vector3> outerIntersections = new List<Vector3>(outerIntersectionsOriginal);
			List<Vector3> innerIntersections = new List<Vector3>(innerIntersectionsOriginal);

			List<Vector3> result = new List<Vector3>();
			Misc.MergeInnerAndOuter(result, outer, inner, outerIntersections, innerIntersections);
			return result;
		} catch (ArgumentOutOfRangeException e) {
			return null;
		}
	}

	private static void MergeInnerAndOuter(List<Vector3> result, List<Vector3> outer, List<Vector3> inner, List<Vector3> outerIntersections, List<Vector3> innerIntersections) {
		int outerIndex = 0;
		int innerIndex = 0;
		bool isOuter = true;

		while (outer.Count > 0 || inner.Count > 0) {
			if (isOuter && outerIndex >= outer.Count && outerIndex > 0) {
				// If this one reaches end, we want to take the rest from the end up to the front
				outerIndex--;
			}
			if (!isOuter && innerIndex >= inner.Count && innerIndex > 0) {
				// If this one reaches end, we want to start over from the beginning
				innerIndex = 0;
			}

			Vector3 current;
			if (isOuter) {
				current = outer[outerIndex];
				outer.RemoveAt(outerIndex);
			} else {
				current = inner[innerIndex];
				inner.RemoveAt(innerIndex);
			}

			result.Add(current);
				// Check if this is the intersection point
			if (((isOuter && outerIntersections.Contains(current)) || (!isOuter && innerIntersections.Contains(current)))) {
				// Pick the closest of the "other" intersection points, and toggle isOuter
				Vector3 closestIntersectionPoint = Misc.GetClosestTo(current, isOuter ? innerIntersections : outerIntersections);
				// Find index of selected intersectionPoint
				int indexOfMatchingIntersection = isOuter ? inner.IndexOf(closestIntersectionPoint) : outer.IndexOf(closestIntersectionPoint);
				if (isOuter) {
					// Special case for first (and only) switch to inner - we don't want "next" index to be the other intersection, if it is - reverse the list
					int nextIndex = indexOfMatchingIntersection + 1;
					if (nextIndex >= inner.Count) {
						nextIndex = 0;
					}
					if (innerIntersections.Contains(inner[nextIndex])) {
						// Reverse it!
						inner.Reverse();
						indexOfMatchingIntersection = inner.IndexOf(closestIntersectionPoint);
					}
					// Remove the intersections we've handled
					outerIntersections.Remove(current);
					innerIntersections.Remove(closestIntersectionPoint);
					// Set the inner index that we're switching to
					innerIndex = indexOfMatchingIntersection;
				} else {
					// Remove the intersections we've handled
					innerIntersections.Remove(current);
					outerIntersections.Remove(closestIntersectionPoint);
					// Set the outer index that we're switching to
					outerIndex = indexOfMatchingIntersection;
				}
				isOuter = !isOuter;
			}
		}
	}

	private static Vector3 GetClosestTo(Vector3 checkPoint, List<Vector3> points) {
		float closestDistance = float.MaxValue;
		int closestIndex = 0;

		for (int i = 0; i < points.Count; i++) {
			Vector3 point = points[i];
			float distance = (checkPoint - point).magnitude;
			if (distance < closestDistance) {
				closestDistance = distance;
				closestIndex = i;
			}
		}

		return points[closestIndex];
	}

	public static void AddGravityToWay(GameObject way) {
		// Add rigidbody and mesh collider, so that they will fall onto the underlying plane
		if (way.GetComponent<Rigidbody>() == null) {
			Rigidbody wayBody = way.AddComponent<Rigidbody>();
			MeshCollider wayCollider = way.AddComponent<MeshCollider>();
			wayCollider.convex = true;
			way.layer = LayerMask.NameToLayer("Ways");
            wayBody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
		}
	}

    public static void ReleaseGravityConstraintsOnWay(List<GameObject> ways) {
        foreach (GameObject way in ways) {
            Rigidbody rigidbody = way.GetComponent<Rigidbody>();
            if (rigidbody != null) {
                rigidbody.constraints = RigidbodyConstraints.None;
            }
        }
    }

    public static void SetGravityState(List<GameObject> gameObjects, bool on = false) {
        foreach (GameObject gameObject in gameObjects) {
            Misc.SetGravityState(gameObject, on);
        }
    }

    public static void SetGravityState(GameObject gameObject, bool on = false) {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        if (rigidbody != null) {
            rigidbody.useGravity = on;
            rigidbody.isKinematic = !on;
        }
	}

    public static void SetAverageZPosition(List<GameObject> gameObjects) {
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;
        foreach (GameObject gameObject in gameObjects) {
            minZ = Mathf.Min(gameObject.transform.localPosition.z, minZ);
            maxZ = Mathf.Max(gameObject.transform.localPosition.z, maxZ);
//            if (gameObject.transform.localPosition.z > 1) {
//                Debug.Log(gameObject.name);
//            }
        }
//		Debug.Break();
//        return;

        float averageZ = minZ + (maxZ - minZ) / 2f;
        foreach (GameObject gameObject in gameObjects) {
            Vector3 position = gameObject.transform.localPosition;
            gameObject.transform.localPosition = new Vector3(position.x, position.y, averageZ);
        }
    }

	public static List<GameObject> FindGameObjectsWithLayer(int layer) {
		List<GameObject> gameObjectsInLayer = new List<GameObject>();
		GameObject[] allGameObjects = GameObject.FindObjectsOfType<GameObject>();
		foreach (GameObject gameObject in allGameObjects) {
			if (gameObject.layer == layer) {
				gameObjectsInLayer.Add(gameObject);
			}
		}
		return gameObjectsInLayer;
	}

	public static float GetZRotation(Vector3 point1, Vector3 point2) {
        Vector3 diff = point2 - point1;
		float zRotation = Mathf.Atan(diff.y / diff.x) * 180f / Mathf.PI;
		return zRotation;
	}

    public static Vector3 NoZ(Vector3 vector) {
        return new Vector3(vector.x, vector.y, 0f);
    }

    public static Vector3 WithZ(Vector3 vectorWithoutZ, Vector3 zVector) {
        return new Vector3(vectorWithoutZ.x, vectorWithoutZ.y, zVector.z);
    }

	public static Vector3 GetMidVector(Vector3 vec1, Vector3 vec2) {
        return vec1 + (vec2 - vec1) / 2f;
	}

	public static Vector3 GetProjectedPointOnLine(Vector3 point, Vector3 linePoint1, Vector3 linePoint2) {
        return Math3d.ProjectPointOnLineSegment(linePoint1, linePoint2, point);
	}

	public static Tuple2<float, float> getOffsetPctFromCenter(Vector3 zoomPoint) {
		float centerX = Screen.width / 2f;
		float centerY = Screen.height / 2f;

		float offsetX = zoomPoint.x - centerX;
		float offsetY = zoomPoint.y - centerY;

		return new Tuple2<float, float>(offsetX / centerX, offsetY / centerY);
	}

    private static Dictionary<string, Coroutine> rotationCoroutines = new Dictionary<string, Coroutine>();
    public static void AnimateRotationTo (string key, GameObject obj, Quaternion toRotation, float time = DEFAULT_ANIMATION_TIME) {
        Quaternion fromRotation = obj.transform.localRotation;
        if (rotationCoroutines.ContainsKey(key)) {
            Singleton<SingletonInstance>.Instance.StopCoroutine (rotationCoroutines[key]);
            rotationCoroutines.Remove(key);
        }
		rotationCoroutines.Add(key, Singleton<SingletonInstance>.Instance.StartCoroutine (AnimateRotation(key, obj, fromRotation, toRotation, time)));
    }

    public static void AnimateRotationFromTo (string key, GameObject obj, Vector3 fromRotation, Vector3 toRotation, float time = DEFAULT_ANIMATION_TIME) {
        if (rotationCoroutines.ContainsKey(key)) {
            Singleton<SingletonInstance>.Instance.StopCoroutine (rotationCoroutines[key]);
            rotationCoroutines.Remove(key);
        }
		rotationCoroutines.Add(key, Singleton<SingletonInstance>.Instance.StartCoroutine (AnimateRotation(key, obj, fromRotation, toRotation, time)));
    }

    private static IEnumerator AnimateRotation (string key, GameObject obj, Vector3 from, Vector3 to, float time) {
		float t = 0f;
		while (t <= 1f) {
			t += Time.unscaledDeltaTime / time;
			Vector3 currentRotation = Vector3.Slerp(from, to, t);
			obj.transform.localRotation = Quaternion.Euler(currentRotation);
			yield return null;
		}
		rotationCoroutines.Remove(key);
	}

    private static IEnumerator AnimateRotation (string key, GameObject obj, Quaternion from, Quaternion to, float time) {
        float t = 0f;
		while (t <= 1f) {
			t += Time.unscaledDeltaTime / time;
			Quaternion currentRotation = Quaternion.Slerp(from, to, t);
            obj.transform.localRotation = currentRotation;
			yield return null;
		}
        rotationCoroutines.Remove(key);
	}

    private static Dictionary<string, Coroutine> movementCoroutines = new Dictionary<string, Coroutine>();
    public static void AnimateMovementTo (string key, GameObject obj, Vector3 toPosition, float time = DEFAULT_ANIMATION_TIME, bool straightMotion = false) {
        Vector3 fromPosition = obj.transform.localPosition;
        if (movementCoroutines.ContainsKey(key)) {
            Singleton<SingletonInstance>.Instance.StopCoroutine (movementCoroutines[key]);
            movementCoroutines.Remove(key);
        }
        movementCoroutines.Add(key, Singleton<SingletonInstance>.Instance.StartCoroutine (AnimateMovement(key, obj, fromPosition, toPosition, time, straightMotion)));
    }

    public static void AnimateRelativeMovement (string baseKey, List<GameObject> gameObjects, Vector3 movement, float time = DEFAULT_ANIMATION_TIME, bool straightMotion = false) {
        int i = 0;
        foreach (GameObject gameObject in gameObjects) {
            string animationKey = baseKey + "_" + i;
            Vector3 toPosition = gameObject.transform.localPosition + movement;
            AnimateMovementTo(animationKey, gameObject, toPosition, time, straightMotion);
            i++;
        }
    }

    public static bool IsAnimationActive (string key) {
        return movementCoroutines.ContainsKey(key);
    }

    private static IEnumerator AnimateMovement (string key, GameObject obj, Vector3 from, Vector3 to, float time, bool straightMotion = false) {
        float t = 0f;
		while (t <= 1f) {
			t += Time.unscaledDeltaTime / time;
			Vector3 currentPosition = straightMotion ? Vector3.Lerp(from, to, t) : Vector3.Slerp(from, to, t);
            obj.transform.localPosition = currentPosition;
			yield return null;
		}
        movementCoroutines.Remove(key);
	}

    private static Dictionary<string, Coroutine> scaleCoroutines = new Dictionary<string, Coroutine>();
    public static void AnimateScaleTo (string key, GameObject obj, Vector3 toScale, float time = DEFAULT_ANIMATION_TIME) {
        Vector3 fromScale = obj.transform.localScale;
        if (scaleCoroutines.ContainsKey(key)) {
            Singleton<SingletonInstance>.Instance.StopCoroutine (scaleCoroutines[key]);
            scaleCoroutines.Remove(key);
        }
        scaleCoroutines.Add(key, Singleton<SingletonInstance>.Instance.StartCoroutine (AnimateScale(key, obj, fromScale, toScale, time)));
    }

    private static IEnumerator AnimateScale (string key, GameObject obj, Vector3 from, Vector3 to, float time) {
        float t = 0f;
		while (t <= 1f) {
			t += Time.unscaledDeltaTime / time;
			Vector3 currentScale = Vector3.Slerp(from, to, t);
            obj.transform.localScale = currentScale;
			yield return null;
		}
        scaleCoroutines.Remove(key);
	}

    private static Dictionary<string, Coroutine> blurCoroutines = new Dictionary<string, Coroutine>();
    public static void AnimateBlurTo (string key, BlurOptimized blurOptimized, int toDownsample, float toSize, int toIterations, float time = DEFAULT_ANIMATION_TIME) {
        int fromDownsample = blurOptimized.downsample;
        float fromSize = blurOptimized.blurSize;
        int fromIterations = blurOptimized.blurIterations;
        if (blurCoroutines.ContainsKey(key)) {
            Singleton<SingletonInstance>.Instance.StopCoroutine (blurCoroutines[key]);
            blurCoroutines.Remove(key);
        }
        blurCoroutines.Add(key, Singleton<SingletonInstance>.Instance.StartCoroutine (AnimateBlur(key, blurOptimized, fromDownsample, fromSize, fromIterations, toDownsample, toSize, toIterations, time)));
    }

    private static IEnumerator AnimateBlur (string key, BlurOptimized blurOptimized, int fromDownsample, float fromSize, int fromIterations, int toDownsample, float toSize, int toIterations, float time) {
        float t = 0f;
		while (t <= 1f) {
			t += Time.unscaledDeltaTime / time;
			float currentDownsample = Mathf.Lerp(fromDownsample, toDownsample, t);
			float currentSize = Mathf.Lerp(fromSize, toSize, t);
			float currentIterations = Mathf.Lerp(fromIterations, toIterations, t);

            blurOptimized.downsample = Mathf.RoundToInt(currentDownsample);
            blurOptimized.blurSize = currentSize;
            blurOptimized.blurIterations = Mathf.RoundToInt(currentIterations);

			yield return null;
		}
        blurCoroutines.Remove(key);
	}

    private static Dictionary<string, Coroutine> fovCoroutines = new Dictionary<string, Coroutine>();
    public static void AnimateFOVTo (string key, Camera camera, float toFOV, float time = DEFAULT_ANIMATION_TIME) {
        float fromFOV = camera.fieldOfView;
        StopAnimateFOV(key);
        fovCoroutines.Add(key, Singleton<SingletonInstance>.Instance.StartCoroutine (AnimateFOVFromTo(key, camera, fromFOV, toFOV, time)));
    }

    private static IEnumerator AnimateFOVFromTo (string key, Camera camera, float fromFOV, float toFOV, float time) {
        float t = 0f;
		while (t <= 1f) {
			t += Time.unscaledDeltaTime / time;
			float currentFOV = Mathf.Lerp(fromFOV, toFOV, t);

            camera.fieldOfView = currentFOV;

			yield return null;
		}
        fovCoroutines.Remove(key);
	}

    public static void StopAnimateFOV (string key) {
        if (fovCoroutines.ContainsKey(key)) {
            Singleton<SingletonInstance>.Instance.StopCoroutine (fovCoroutines[key]);
            fovCoroutines.Remove(key);
        }
    }

	private static Dictionary<string, Coroutine> genericCoroutines = new Dictionary<string, Coroutine>();
	public static void SetCoroutine (string key, Coroutine coroutine) {
		Misc.StopCoroutine(key);
		genericCoroutines.Add(key, coroutine);
	}

	public static bool HasCoroutine (string key) {
		return genericCoroutines.ContainsKey(key);
	}

	public static void StopCoroutine (string key) {
		if (genericCoroutines.ContainsKey(key)) {
			Singleton<SingletonInstance>.Instance.StopCoroutine (genericCoroutines[key]);
			genericCoroutines.Remove(key);
		}
	}

}