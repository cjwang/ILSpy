﻿using System;

namespace ICSharpCode.Decompiler.Tests.TestCases.Correctness
{
	public struct MutValueType : IDisposable
	{
		public int val;
		
		public void Increment()
		{
			Console.WriteLine("Inc() called on {0}", val);
			val = val + 1;
		}

		public void Dispose()
		{
			Console.WriteLine("MutValueType disposed on {0}", val);
		}
	}
	
	public struct GenericValueType<T>
	{
		T data;
		int num;
		
		public GenericValueType(T data)
		{
			this.data = data;
			this.num = 1;
		}
		
		public void Call(ref GenericValueType<T> v)
		{
			num++;
			Console.WriteLine("Call #{0}: {1} with v=#{2}", num, data, v.num);
		}
	}
	
	public struct ValueTypeWithReadOnlyMember
	{
		public readonly int Member;
		
		public ValueTypeWithReadOnlyMember(int member)
		{
			this.Member = member;
		}
	}
	
	public class ValueTypeCall
	{
		public static void Main()
		{
			MutValueType m = new MutValueType();
			RefParameter(ref m);
			ValueParameter(m);
			Field();
			Box();
			Using();
			var gvt = new GenericValueType<string>("Test");
			gvt.Call(ref gvt);
			new ValueTypeCall().InstanceFieldTests();
		}
		
		static void RefParameter(ref MutValueType m)
		{
			m.Increment();
			m.Increment();
		}
		
		static void ValueParameter(MutValueType m)
		{
			m.Increment();
			m.Increment();
		}
		
		static readonly MutValueType ReadonlyField = new MutValueType { val = 100 };
		static MutValueType MutableField = new MutValueType { val = 200 };
		
		static void Field()
		{
			ReadonlyField.Increment();
			ReadonlyField.Increment();
			MutableField.Increment();
			MutableField.Increment();
			// Ensure that 'v' isn't incorrectly removed
			// as a compiler-generated temporary
			MutValueType v = MutableField;
			v.Increment();
			Console.WriteLine("Final value in MutableField: " + MutableField.val);
			// Read-only field copies cannot be inlined for static methods:
			MutValueType localCopy = ReadonlyField;
			RefParameter(ref localCopy);
		}
		
		static void Box()
		{
			Console.WriteLine("Box");
			object o = new MutValueType { val = 300 };
			((MutValueType)o).Increment();
			((MutValueType)o).Increment();
			MutValueType unboxed1 = (MutValueType)o;
			unboxed1.Increment();
			unboxed1.Increment();
			((MutValueType)o).Increment();
			MutValueType unboxed2 = (MutValueType)o;
			unboxed2.val = 100;
			((MutValueType)o).Dispose();
		}

		MutValueType instanceField;
		ValueTypeWithReadOnlyMember mutableInstanceFieldWithReadOnlyMember;
		
		void InstanceFieldTests()
		{
			this.instanceField.val = 42;
			Console.WriteLine(this.instanceField.val);
			mutableInstanceFieldWithReadOnlyMember = new ValueTypeWithReadOnlyMember(45);
			Console.WriteLine(this.mutableInstanceFieldWithReadOnlyMember.Member);
		}

		static void Using()
		{
			Using1();
			Using2();
			Using3();
		}

		static void Using1()
		{
			Console.WriteLine("Using:");
			using (var x = new MutValueType()) {
				x.Increment();
			}
		}

		static void Using2()
		{
			Console.WriteLine("Not using:");
			var y = new MutValueType();
			try {
				y.Increment();
			} finally {
				MutValueType x = y;
				x.Dispose();
			}
		}

		static void Using3()
		{
			Console.WriteLine("Using with variable declared outside:");
			MutValueType z;
			using (z = new MutValueType()) {
				z.Increment();
			}
		}
	}
}
