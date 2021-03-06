using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Graphics
{
    [DebuggerDisplay("{ParameterClass} {ParameterType} {Name} : {Semantic}")]
    public class EffectParameter
    {
        /// <summary>
        /// The next state key used when an effect parameter
        /// is updated by any of the 'set' methods.
        /// </summary>
        internal static ulong NextStateKey { get; private set; }

        internal EffectParameter(EffectParameterClass class_,
                                    EffectParameterType type,
                                    string name,
                                    int rowCount,
                                    int columnCount,
                                    int registerCount,
                                    string semantic,
                                    EffectAnnotationCollection annotations,
                                    EffectParameterCollection elements,
                                    EffectParameterCollection structMembers,
                                    object data)
        {
            ParameterClass = class_;
            ParameterType = type;

            Name = name;
            Semantic = semantic;
            Annotations = annotations;

            RowCount = rowCount;
            ColumnCount = columnCount;

            RegisterCount = registerCount;

            Elements = elements;
            StructureMembers = structMembers;

            Data = data;
            StateKey = unchecked(NextStateKey++);
        }

        internal EffectParameter(EffectParameter cloneSource)
        {
            // Share all the immutable types.
            ParameterClass = cloneSource.ParameterClass;
            ParameterType = cloneSource.ParameterType;
            Name = cloneSource.Name;
            Semantic = cloneSource.Semantic;
            Annotations = cloneSource.Annotations;
            RowCount = cloneSource.RowCount;
            ColumnCount = cloneSource.ColumnCount;
            RegisterCount = cloneSource.RegisterCount;

            // Clone the mutable types.
            Elements = new EffectParameterCollection(cloneSource.Elements);
            StructureMembers = new EffectParameterCollection(cloneSource.StructureMembers);

            // Data is mutable, but a new copy happens during
            // boxing/unboxing so we can just assign it.
            Data = cloneSource.Data is Array ? (cloneSource.Data as Array).Clone() : cloneSource.Data;
            StateKey = unchecked(NextStateKey++);
        }

        public string Name { get; private set; }

        public string Semantic { get; private set; }

        public EffectParameterClass ParameterClass { get; private set; }

        public EffectParameterType ParameterType { get; private set; }

        public int RowCount { get; private set; }

        public int ColumnCount { get; private set; }

        internal int RegisterCount { get; private set; }

        public EffectParameterCollection Elements { get; private set; }

        public EffectParameterCollection StructureMembers { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }


        // TODO: Using object adds alot of boxing/unboxing overhead
        // and garbage generation.  We should consider a templated
        // type implementation to fix this!

        internal object Data { get; private set; }

        /// <summary>
        /// The current state key which is used to detect
        /// if the parameter value has been changed.
        /// </summary>
        internal ulong StateKey { get; private set; }

        public void SetValue(object value)
        {
            throw new NotImplementedException();
        }

        public bool GetValueBoolean()
        {
            if (ParameterClass != EffectParameterClass.Scalar || ParameterType != EffectParameterType.Bool)
                throw new InvalidCastException();

#if DIRECTX
            return ((int[])Data)[0] != 0;
#else
            // MojoShader encodes even booleans into a float.
            return ((float[])Data)[0] != 0.0f;
#endif
        }

        public bool[] GetValueBooleanArray()
        {
            throw new NotImplementedException();
        }

        public int GetValueInt32()
        {
            if (ParameterClass != EffectParameterClass.Scalar || ParameterType != EffectParameterType.Int32)
                throw new InvalidCastException();

#if DIRECTX
            return ((int[])Data)[0];
#else
            // MojoShader encodes integers into a float.
            return (int)((float[])Data)[0];
#endif
        }

        public int[] GetValueInt32Array()
        {
            throw new NotImplementedException();
        }

        public Matrix GetValueMatrix()
        {
            if (RowCount == 4 && ColumnCount == 4)
            {
                var floatData = (float[])Data;
                return new Matrix(floatData[0], floatData[4], floatData[8], floatData[12],
                    floatData[1], floatData[5], floatData[9], floatData[13],
                    floatData[2], floatData[6], floatData[10], floatData[14],
                    floatData[3], floatData[7], floatData[11], floatData[15]);
            }
            throw new Exception("Matrix data incorrect or missing!");
        }

        public Matrix[] GetValueMatrixArray(int count)
        {
            throw new NotImplementedException();
        }

        public Quaternion GetValueQuaternion()
        {
            throw new NotImplementedException();
        }

        public Quaternion[] GetValueQuaternionArray()
        {
            throw new NotImplementedException();
        }

        public Single GetValueSingle()
        {
            switch (ParameterType)
            {
                case EffectParameterType.Int32:
                    return (Single)(int)Data;
                default:
                    return (Single)Data;
            }
        }

        public Single[] GetValueSingleArray()
        {
            if (Elements != null && Elements.Count > 0)
            {
                var ret = new Single[RowCount * ColumnCount * Elements.Count];
                for (int i = 0; i < Elements.Count; i++)
                {
                    var elmArray = Elements[i].GetValueSingleArray();
                    for (var j = 0; j < elmArray.Length; j++)
                        ret[RowCount * ColumnCount * i + j] = elmArray[j];
                }
                return ret;
            }

            switch (ParameterClass)
            {
                case EffectParameterClass.Scalar:
                    return new Single[] { GetValueSingle() };
                case EffectParameterClass.Vector:
                case EffectParameterClass.Matrix:
                    if (Data is Matrix)
                        return Matrix.ToFloatArray((Matrix)Data);
                    else
                        return (float[])Data;
                default:
                    throw new NotImplementedException();
            }
        }

        public string GetValueString()
        {
            if (ParameterClass != EffectParameterClass.Object || ParameterType != EffectParameterType.String)
                throw new InvalidCastException();

		    return ((string[])Data)[0];
        }

        public Texture2D GetValueTexture2D()
        {
            if (ParameterClass != EffectParameterClass.Object || ParameterType != EffectParameterType.Texture2D)
                throw new InvalidCastException();

			return (Texture2D)Data;
		}

#if !GLES
	    public Texture3D GetValueTexture3D()
	    {
            if (ParameterClass != EffectParameterClass.Object || ParameterType != EffectParameterType.Texture3D)
                throw new InvalidCastException();

            return (Texture3D)Data;
	    }
#endif

        public TextureCube GetValueTextureCube()
        {
            throw new NotImplementedException();
        }

        public Vector2 GetValueVector2()
        {
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var vecInfo = (float[])Data;
            return new Vector2(vecInfo[0], vecInfo[1]);
        }

        public Vector2[] GetValueVector2Array()
        {
            throw new NotImplementedException();
        }

        public Vector3 GetValueVector3()
        {
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var vecInfo = (float[])Data;
            return new Vector3(vecInfo[0], vecInfo[1], vecInfo[2]);
        }

        public Vector3[] GetValueVector3Array()
        {
            throw new NotImplementedException();
        }

        public Vector4 GetValueVector4()
        {
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var vecInfo = (float[])Data;
            return new Vector4(vecInfo[0], vecInfo[1], vecInfo[2], vecInfo[3]);
        }

        public Vector4[] GetValueVector4Array()
        {
            throw new NotImplementedException();
        }

        public void SetValue(bool value)
        {
            if (ParameterClass != EffectParameterClass.Scalar || ParameterType != EffectParameterType.Bool)
                throw new InvalidCastException();

#if DIRECTX
            // We store the bool as an integer as that
            // is what the constant buffers expect.
            ((int[])Data)[0] = value ? 1 : 0;
#else
            // MojoShader encodes even booleans into a float.
            ((float[])Data)[0] = value ? 1 : 0;
#endif
            StateKey = unchecked(NextStateKey++);
		}

        public void SetValue(bool[] value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(int value)
        {
            Data = value;
            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(int[] value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(Matrix value)
        {
            // HLSL expects matrices to be transposed by default.
            // These unrolled loops do the transpose during assignment.
            if (RowCount == 4 && ColumnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;
                fData[3] = value.M41;

                fData[4] = value.M12;
                fData[5] = value.M22;
                fData[6] = value.M32;
                fData[7] = value.M42;

                fData[8] = value.M13;
                fData[9] = value.M23;
                fData[10] = value.M33;
                fData[11] = value.M43;

                fData[12] = value.M14;
                fData[13] = value.M24;
                fData[14] = value.M34;
                fData[15] = value.M44;
            }
            else if (RowCount == 4 && ColumnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;
                fData[3] = value.M41;

                fData[4] = value.M12;
                fData[5] = value.M22;
                fData[6] = value.M32;
                fData[7] = value.M42;

                fData[8] = value.M13;
                fData[9] = value.M23;
                fData[10] = value.M33;
                fData[11] = value.M43;
            }
            else if (RowCount == 3 && ColumnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;

                fData[6] = value.M13;
                fData[7] = value.M23;
                fData[8] = value.M33;

                fData[9] = value.M14;
                fData[10] = value.M24;
                fData[11] = value.M34;
            }
            else if (RowCount == 3 && ColumnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M21;
                fData[2] = value.M31;

                fData[3] = value.M12;
                fData[4] = value.M22;
                fData[5] = value.M32;

                fData[6] = value.M13;
                fData[7] = value.M23;
                fData[8] = value.M33;
            }

            StateKey = unchecked(NextStateKey++);
        }

        public void SetValueTranspose(Matrix value)
        {
            // HLSL expects matrices to be transposed by default, so copying them straight
            // from the in-memory version effectively transposes them back to row-major.
            if (RowCount == 4 && ColumnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;
                fData[3] = value.M14;

                fData[4] = value.M21;
                fData[5] = value.M22;
                fData[6] = value.M23;
                fData[7] = value.M24;

                fData[8] = value.M31;
                fData[9] = value.M32;
                fData[10] = value.M33;
                fData[11] = value.M34;

                fData[12] = value.M41;
                fData[13] = value.M42;
                fData[14] = value.M43;
                fData[15] = value.M44;
            }
            else if (RowCount == 4 && ColumnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;

                fData[3] = value.M21;
                fData[4] = value.M22;
                fData[5] = value.M23;

                fData[6] = value.M31;
                fData[7] = value.M32;
                fData[8] = value.M33;

                fData[9] = value.M41;
                fData[10] = value.M42;
                fData[11] = value.M43;
            }
            else if (RowCount == 3 && ColumnCount == 4)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;
                fData[3] = value.M14;

                fData[4] = value.M21;
                fData[5] = value.M22;
                fData[6] = value.M23;
                fData[7] = value.M24;

                fData[8] = value.M31;
                fData[9] = value.M32;
                fData[10] = value.M33;
                fData[11] = value.M34;
            }
            else if (RowCount == 3 && ColumnCount == 3)
            {
                var fData = (float[])Data;

                fData[0] = value.M11;
                fData[1] = value.M12;
                fData[2] = value.M13;

                fData[3] = value.M21;
                fData[4] = value.M22;
                fData[5] = value.M23;

                fData[6] = value.M31;
                fData[7] = value.M32;
                fData[8] = value.M33;
            }

            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Matrix[] value)
        {
            var arrayData = Data as float[];
            if (arrayData == null || arrayData.Length < value.Length * 16)
                Data = arrayData = new float[value.Length * 16];

            for (int i = 0; i < value.Length; i++)
            {
                var m = value[i];

                arrayData[i * 16] = m.M11;
                arrayData[i * 16 + 1] = m.M21;
                arrayData[i * 16 + 2] = m.M31;
                arrayData[i * 16 + 3] = m.M41;

                arrayData[i * 16 + 4] = m.M12;
                arrayData[i * 16 + 5] = m.M22;
                arrayData[i * 16 + 6] = m.M32;
                arrayData[i * 16 + 7] = m.M42;

                arrayData[i * 16 + 8] = m.M13;
                arrayData[i * 16 + 9] = m.M23;
                arrayData[i * 16 + 10] = m.M33;
                arrayData[i * 16 + 11] = m.M43;

                arrayData[i * 16 + 12] = m.M14;
                arrayData[i * 16 + 13] = m.M24;
                arrayData[i * 16 + 14] = m.M34;
                arrayData[i * 16 + 15] = m.M44;
            }

            StateKey = unchecked(NextStateKey++);
        }
        public void SetValue(Matrix[] value, int length)
        {
            var arrayData = Data as float[];
            if (arrayData == null || arrayData.Length < length * 16)
                Data = arrayData = new float[length * 16];

            for (int i = 0; i < length; i++)
            {
                var m = value[i];

                arrayData[i * 16] = m.M11;
                arrayData[i * 16 + 1] = m.M21;
                arrayData[i * 16 + 2] = m.M31;
                arrayData[i * 16 + 3] = m.M41;

                arrayData[i * 16 + 4] = m.M12;
                arrayData[i * 16 + 5] = m.M22;
                arrayData[i * 16 + 6] = m.M32;
                arrayData[i * 16 + 7] = m.M42;

                arrayData[i * 16 + 8] = m.M13;
                arrayData[i * 16 + 9] = m.M23;
                arrayData[i * 16 + 10] = m.M33;
                arrayData[i * 16 + 11] = m.M43;

                arrayData[i * 16 + 12] = m.M14;
                arrayData[i * 16 + 13] = m.M24;
                arrayData[i * 16 + 14] = m.M34;
                arrayData[i * 16 + 15] = m.M44;
            }

            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Quaternion value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(Quaternion[] value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(Single value)
        {
            switch (ParameterClass)
            {
                case EffectParameterClass.Matrix:
                case EffectParameterClass.Vector:
                    ((float[])Data)[0] = value;
                    break;
                case EffectParameterClass.Scalar:
                    Data = value;
                    break;
                default:
                    throw new NotImplementedException();
            }

            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Single[] value)
        {
            var arrayData = Data as float[];
            if (arrayData == null || arrayData.Length < value.Length)
                Data = arrayData = new float[value.Length];

            Array.Copy(value, arrayData, value.Length);
            
            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(string value)
        {
            throw new NotImplementedException();
        }

        public void SetValue(Texture value)
        {
            if (ParameterClass != EffectParameterClass.Object)
                throw new InvalidCastException();

            if (value != null)
            {
                if (value is Texture2D)
                {
                    if (ParameterType != EffectParameterType.Texture1D &&
                        ParameterType != EffectParameterType.Texture2D)
                        throw new InvalidCastException();
                }
#if !GLES
                else if (value is Texture3D)
                {
                    if (ParameterType != EffectParameterType.Texture3D)
                        throw new InvalidCastException();
                }
#endif
                else
                {
                    if (!(value is TextureCube) || ParameterType != EffectParameterType.TextureCube)
                        throw new InvalidCastException();
                }
            }

            Data = value;
            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Vector2 value)
        {
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Vector2[] value)
        {
            for (var i = 0; i < value.Length; i++)
                Elements[i].SetValue(value[i]);
            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Vector3 value)
        {
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            fData[2] = value.Z;
            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Vector3[] value)
        {
            for (var i = 0; i < value.Length; i++)
                Elements[i].SetValue(value[i]);
            StateKey = unchecked(NextStateKey++);
        }

        public void SetValue(Vector4 value)
        {
            if (ParameterClass != EffectParameterClass.Vector || ParameterType != EffectParameterType.Single)
                throw new InvalidCastException();

            var fData = (float[])Data;
            fData[0] = value.X;
            fData[1] = value.Y;
            fData[2] = value.Z;
            fData[3] = value.W;
            StateKey = unchecked(NextStateKey++);
        }

        public unsafe void SetValue(Vector4[] value)
        {
            var arrayData = Data as float[];
            if (arrayData == null || arrayData.Length < value.Length * 4)
                Data = arrayData = new float[value.Length * 4];

            fixed (Vector4* pValue = value)
            fixed (float* pData = arrayData)
            {
                var pValueAsFloat = (float*)pValue;
                var dataIt = pData;

                for (int i = 0; i < value.Length * 4; i++)
                {
                    *dataIt = *pValueAsFloat;
                    pValueAsFloat++; dataIt++;
                }
            }

            StateKey = unchecked(NextStateKey++);
        }
        public unsafe void SetValue(Vector4[] value, int length)
        {
            var arrayData = Data as float[];
            if (arrayData == null || arrayData.Length < length * 4)
                Data = arrayData = new float[length * 4];

            fixed (Vector4* pValue = value)
            fixed (float* pData = arrayData)
            {
                var pValueAsFloat = (float*)pValue;
                var dataIt = pData;

                for (int i = 0; i < length * 4; i++)
                {
                    *dataIt = *pValueAsFloat;
                    pValueAsFloat++; dataIt++;
                }
            }

            StateKey = unchecked(NextStateKey++);
        }
    }
}
