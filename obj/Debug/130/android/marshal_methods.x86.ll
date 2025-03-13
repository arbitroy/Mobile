; ModuleID = 'obj\Debug\130\android\marshal_methods.x86.ll'
source_filename = "obj\Debug\130\android\marshal_methods.x86.ll"
target datalayout = "e-m:e-p:32:32-p270:32:32-p271:32:32-p272:64:64-f64:32:64-f80:32-n8:16:32-S128"
target triple = "i686-unknown-linux-android"


%struct.MonoImage = type opaque

%struct.MonoClass = type opaque

%struct.MarshalMethodsManagedClass = type {
	i32,; uint32_t token
	%struct.MonoClass*; MonoClass* klass
}

%struct.MarshalMethodName = type {
	i64,; uint64_t id
	i8*; char* name
}

%class._JNIEnv = type opaque

%class._jobject = type {
	i8; uint8_t b
}

%class._jclass = type {
	i8; uint8_t b
}

%class._jstring = type {
	i8; uint8_t b
}

%class._jthrowable = type {
	i8; uint8_t b
}

%class._jarray = type {
	i8; uint8_t b
}

%class._jobjectArray = type {
	i8; uint8_t b
}

%class._jbooleanArray = type {
	i8; uint8_t b
}

%class._jbyteArray = type {
	i8; uint8_t b
}

%class._jcharArray = type {
	i8; uint8_t b
}

%class._jshortArray = type {
	i8; uint8_t b
}

%class._jintArray = type {
	i8; uint8_t b
}

%class._jlongArray = type {
	i8; uint8_t b
}

%class._jfloatArray = type {
	i8; uint8_t b
}

%class._jdoubleArray = type {
	i8; uint8_t b
}

; assembly_image_cache
@assembly_image_cache = local_unnamed_addr global [0 x %struct.MonoImage*] zeroinitializer, align 4
; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = local_unnamed_addr constant [146 x i32] [
	i32 32687329, ; 0: Xamarin.AndroidX.Lifecycle.Runtime => 0x1f2c4e1 => 41
	i32 34715100, ; 1: Xamarin.Google.Guava.ListenableFuture.dll => 0x211b5dc => 59
	i32 39109920, ; 2: Newtonsoft.Json.dll => 0x254c520 => 4
	i32 57967248, ; 3: Xamarin.Android.Support.VersionedParcelable.dll => 0x3748290 => 18
	i32 101534019, ; 4: Xamarin.AndroidX.SlidingPaneLayout => 0x60d4943 => 50
	i32 120558881, ; 5: Xamarin.AndroidX.SlidingPaneLayout.dll => 0x72f9521 => 50
	i32 160529393, ; 6: Xamarin.Android.Arch.Core.Common => 0x9917bf1 => 12
	i32 165246403, ; 7: Xamarin.AndroidX.Collection.dll => 0x9d975c3 => 28
	i32 166922606, ; 8: Xamarin.Android.Support.Compat.dll => 0x9f3096e => 17
	i32 182336117, ; 9: Xamarin.AndroidX.SwipeRefreshLayout.dll => 0xade3a75 => 51
	i32 209399409, ; 10: Xamarin.AndroidX.Browser.dll => 0xc7b2e71 => 26
	i32 230216969, ; 11: Xamarin.AndroidX.Legacy.Support.Core.Utils.dll => 0xdb8d509 => 38
	i32 232815796, ; 12: System.Web.Services => 0xde07cb4 => 70
	i32 280482487, ; 13: Xamarin.AndroidX.Interpolator => 0x10b7d2b7 => 36
	i32 318968648, ; 14: Xamarin.AndroidX.Activity.dll => 0x13031348 => 19
	i32 321597661, ; 15: System.Numerics => 0x132b30dd => 8
	i32 342366114, ; 16: Xamarin.AndroidX.Lifecycle.Common => 0x146817a2 => 39
	i32 442521989, ; 17: Xamarin.Essentials => 0x1a605985 => 57
	i32 450948140, ; 18: Xamarin.AndroidX.Fragment.dll => 0x1ae0ec2c => 35
	i32 465846621, ; 19: mscorlib => 0x1bc4415d => 3
	i32 469710990, ; 20: System.dll => 0x1bff388e => 6
	i32 476646585, ; 21: Xamarin.AndroidX.Interpolator.dll => 0x1c690cb9 => 36
	i32 486930444, ; 22: Xamarin.AndroidX.LocalBroadcastManager.dll => 0x1d05f80c => 45
	i32 514659665, ; 23: Xamarin.Android.Support.Compat => 0x1ead1551 => 17
	i32 526420162, ; 24: System.Transactions.dll => 0x1f6088c2 => 63
	i32 605376203, ; 25: System.IO.Compression.FileSystem => 0x24154ecb => 66
	i32 627609679, ; 26: Xamarin.AndroidX.CustomView => 0x2568904f => 32
	i32 663517072, ; 27: Xamarin.AndroidX.VersionedParcelable => 0x278c7790 => 55
	i32 666292255, ; 28: Xamarin.AndroidX.Arch.Core.Common.dll => 0x27b6d01f => 23
	i32 690569205, ; 29: System.Xml.Linq.dll => 0x29293ff5 => 71
	i32 692692150, ; 30: Xamarin.Android.Support.Annotations => 0x2949a4b6 => 15
	i32 775507847, ; 31: System.IO.Compression => 0x2e394f87 => 65
	i32 809851609, ; 32: System.Drawing.Common.dll => 0x30455ad9 => 60
	i32 843511501, ; 33: Xamarin.AndroidX.Print => 0x3246f6cd => 47
	i32 928116545, ; 34: Xamarin.Google.Guava.ListenableFuture => 0x3751ef41 => 59
	i32 955402788, ; 35: Newtonsoft.Json => 0x38f24a24 => 4
	i32 967690846, ; 36: Xamarin.AndroidX.Lifecycle.Common.dll => 0x39adca5e => 39
	i32 1012816738, ; 37: Xamarin.AndroidX.SavedState.dll => 0x3c5e5b62 => 49
	i32 1035644815, ; 38: Xamarin.AndroidX.AppCompat => 0x3dbaaf8f => 22
	i32 1052210849, ; 39: Xamarin.AndroidX.Lifecycle.ViewModel.dll => 0x3eb776a1 => 42
	i32 1098259244, ; 40: System => 0x41761b2c => 6
	i32 1175144683, ; 41: Xamarin.AndroidX.VectorDrawable.Animated => 0x460b48eb => 53
	i32 1204270330, ; 42: Xamarin.AndroidX.Arch.Core.Common => 0x47c7b4fa => 23
	i32 1267360935, ; 43: Xamarin.AndroidX.VectorDrawable => 0x4b8a64a7 => 54
	i32 1293217323, ; 44: Xamarin.AndroidX.DrawerLayout.dll => 0x4d14ee2b => 34
	i32 1365406463, ; 45: System.ServiceModel.Internals.dll => 0x516272ff => 69
	i32 1376866003, ; 46: Xamarin.AndroidX.SavedState => 0x52114ed3 => 49
	i32 1406073936, ; 47: Xamarin.AndroidX.CoordinatorLayout => 0x53cefc50 => 29
	i32 1462112819, ; 48: System.IO.Compression.dll => 0x57261233 => 65
	i32 1469204771, ; 49: Xamarin.AndroidX.AppCompat.AppCompatResources => 0x57924923 => 21
	i32 1582372066, ; 50: Xamarin.AndroidX.DocumentFile.dll => 0x5e5114e2 => 33
	i32 1587447679, ; 51: Xamarin.Android.Arch.Core.Common.dll => 0x5e9e877f => 12
	i32 1592978981, ; 52: System.Runtime.Serialization.dll => 0x5ef2ee25 => 68
	i32 1622152042, ; 53: Xamarin.AndroidX.Loader.dll => 0x60b0136a => 44
	i32 1636350590, ; 54: Xamarin.AndroidX.CursorAdapter => 0x6188ba7e => 31
	i32 1639515021, ; 55: System.Net.Http.dll => 0x61b9038d => 7
	i32 1657153582, ; 56: System.Runtime => 0x62c6282e => 10
	i32 1658251792, ; 57: Xamarin.Google.Android.Material.dll => 0x62d6ea10 => 58
	i32 1729485958, ; 58: Xamarin.AndroidX.CardView.dll => 0x6715dc86 => 27
	i32 1766324549, ; 59: Xamarin.AndroidX.SwipeRefreshLayout => 0x6947f945 => 51
	i32 1776026572, ; 60: System.Core.dll => 0x69dc03cc => 5
	i32 1788241197, ; 61: Xamarin.AndroidX.Fragment => 0x6a96652d => 35
	i32 1808609942, ; 62: Xamarin.AndroidX.Loader => 0x6bcd3296 => 44
	i32 1813201214, ; 63: Xamarin.Google.Android.Material => 0x6c13413e => 58
	i32 1867746548, ; 64: Xamarin.Essentials.dll => 0x6f538cf4 => 57
	i32 1885316902, ; 65: Xamarin.AndroidX.Arch.Core.Runtime.dll => 0x705fa726 => 24
	i32 1919157823, ; 66: Xamarin.AndroidX.MultiDex.dll => 0x7264063f => 46
	i32 2019465201, ; 67: Xamarin.AndroidX.Lifecycle.ViewModel => 0x785e97f1 => 42
	i32 2055257422, ; 68: Xamarin.AndroidX.Lifecycle.LiveData.Core.dll => 0x7a80bd4e => 40
	i32 2079903147, ; 69: System.Runtime.dll => 0x7bf8cdab => 10
	i32 2090596640, ; 70: System.Numerics.Vectors => 0x7c9bf920 => 9
	i32 2097448633, ; 71: Xamarin.AndroidX.Legacy.Support.Core.UI => 0x7d0486b9 => 37
	i32 2196165013, ; 72: Xamarin.Android.Support.VersionedParcelable => 0x82e6d195 => 18
	i32 2201231467, ; 73: System.Net.Http => 0x8334206b => 7
	i32 2217644978, ; 74: Xamarin.AndroidX.VectorDrawable.Animated.dll => 0x842e93b2 => 53
	i32 2244775296, ; 75: Xamarin.AndroidX.LocalBroadcastManager => 0x85cc8d80 => 45
	i32 2256548716, ; 76: Xamarin.AndroidX.MultiDex => 0x8680336c => 46
	i32 2279755925, ; 77: Xamarin.AndroidX.RecyclerView.dll => 0x87e25095 => 48
	i32 2315684594, ; 78: Xamarin.AndroidX.Annotation.dll => 0x8a068af2 => 20
	i32 2471841756, ; 79: netstandard.dll => 0x93554fdc => 61
	i32 2475788418, ; 80: Java.Interop.dll => 0x93918882 => 1
	i32 2501346920, ; 81: System.Data.DataSetExtensions => 0x95178668 => 64
	i32 2505896520, ; 82: Xamarin.AndroidX.Lifecycle.Runtime.dll => 0x955cf248 => 41
	i32 2581819634, ; 83: Xamarin.AndroidX.VectorDrawable.dll => 0x99e370f2 => 54
	i32 2620871830, ; 84: Xamarin.AndroidX.CursorAdapter.dll => 0x9c375496 => 31
	i32 2732626843, ; 85: Xamarin.AndroidX.Activity => 0xa2e0939b => 19
	i32 2737747696, ; 86: Xamarin.AndroidX.AppCompat.AppCompatResources.dll => 0xa32eb6f0 => 21
	i32 2751899777, ; 87: Xamarin.Android.Support.Collections => 0xa406a881 => 16
	i32 2778768386, ; 88: Xamarin.AndroidX.ViewPager.dll => 0xa5a0a402 => 56
	i32 2810250172, ; 89: Xamarin.AndroidX.CoordinatorLayout.dll => 0xa78103bc => 29
	i32 2819470561, ; 90: System.Xml.dll => 0xa80db4e1 => 11
	i32 2853208004, ; 91: Xamarin.AndroidX.ViewPager => 0xaa107fc4 => 56
	i32 2855708567, ; 92: Xamarin.AndroidX.Transition => 0xaa36a797 => 52
	i32 2903344695, ; 93: System.ComponentModel.Composition => 0xad0d8637 => 67
	i32 2905242038, ; 94: mscorlib.dll => 0xad2a79b6 => 3
	i32 2919462931, ; 95: System.Numerics.Vectors.dll => 0xae037813 => 9
	i32 2978675010, ; 96: Xamarin.AndroidX.DrawerLayout => 0xb18af942 => 34
	i32 3024354802, ; 97: Xamarin.AndroidX.Legacy.Support.Core.Utils => 0xb443fdf2 => 38
	i32 3068715062, ; 98: Xamarin.Android.Arch.Lifecycle.Common => 0xb6e8e036 => 13
	i32 3111772706, ; 99: System.Runtime.Serialization => 0xb979e222 => 68
	i32 3204380047, ; 100: System.Data.dll => 0xbefef58f => 62
	i32 3211777861, ; 101: Xamarin.AndroidX.DocumentFile => 0xbf6fd745 => 33
	i32 3247949154, ; 102: Mono.Security => 0xc197c562 => 72
	i32 3258312781, ; 103: Xamarin.AndroidX.CardView => 0xc235e84d => 27
	i32 3267021929, ; 104: Xamarin.AndroidX.AsyncLayoutInflater => 0xc2bacc69 => 25
	i32 3296380511, ; 105: Xamarin.Android.Support.Collections.dll => 0xc47ac65f => 16
	i32 3317135071, ; 106: Xamarin.AndroidX.CustomView.dll => 0xc5b776df => 32
	i32 3317144872, ; 107: System.Data => 0xc5b79d28 => 62
	i32 3340431453, ; 108: Xamarin.AndroidX.Arch.Core.Runtime => 0xc71af05d => 24
	i32 3353484488, ; 109: Xamarin.AndroidX.Legacy.Support.Core.UI.dll => 0xc7e21cc8 => 37
	i32 3362522851, ; 110: Xamarin.AndroidX.Core => 0xc86c06e3 => 30
	i32 3366347497, ; 111: Java.Interop => 0xc8a662e9 => 1
	i32 3374999561, ; 112: Xamarin.AndroidX.RecyclerView => 0xc92a6809 => 48
	i32 3404865022, ; 113: System.ServiceModel.Internals => 0xcaf21dfe => 69
	i32 3429136800, ; 114: System.Xml => 0xcc6479a0 => 11
	i32 3430777524, ; 115: netstandard => 0xcc7d82b4 => 61
	i32 3439690031, ; 116: Xamarin.Android.Support.Annotations.dll => 0xcd05812f => 15
	i32 3476120550, ; 117: Mono.Android => 0xcf3163e6 => 2
	i32 3486566296, ; 118: System.Transactions => 0xcfd0c798 => 63
	i32 3501239056, ; 119: Xamarin.AndroidX.AsyncLayoutInflater.dll => 0xd0b0ab10 => 25
	i32 3509114376, ; 120: System.Xml.Linq => 0xd128d608 => 71
	i32 3567349600, ; 121: System.ComponentModel.Composition.dll => 0xd4a16f60 => 67
	i32 3627220390, ; 122: Xamarin.AndroidX.Print.dll => 0xd832fda6 => 47
	i32 3641597786, ; 123: Xamarin.AndroidX.Lifecycle.LiveData.Core => 0xd90e5f5a => 40
	i32 3672681054, ; 124: Mono.Android.dll => 0xdae8aa5e => 2
	i32 3676310014, ; 125: System.Web.Services.dll => 0xdb2009fe => 70
	i32 3681174138, ; 126: Xamarin.Android.Arch.Lifecycle.Common.dll => 0xdb6a427a => 13
	i32 3682565725, ; 127: Xamarin.AndroidX.Browser => 0xdb7f7e5d => 26
	i32 3689375977, ; 128: System.Drawing.Common => 0xdbe768e9 => 60
	i32 3718780102, ; 129: Xamarin.AndroidX.Annotation => 0xdda814c6 => 20
	i32 3721776493, ; 130: Mobile => 0xddd5cd6d => 0
	i32 3765952165, ; 131: Mobile.dll => 0xe077dea5 => 0
	i32 3786282454, ; 132: Xamarin.AndroidX.Collection => 0xe1ae15d6 => 28
	i32 3829621856, ; 133: System.Numerics.dll => 0xe4436460 => 8
	i32 3862817207, ; 134: Xamarin.Android.Arch.Lifecycle.Runtime.dll => 0xe63de9b7 => 14
	i32 3874897629, ; 135: Xamarin.Android.Arch.Lifecycle.Runtime => 0xe6f63edd => 14
	i32 3885922214, ; 136: Xamarin.AndroidX.Transition.dll => 0xe79e77a6 => 52
	i32 3896760992, ; 137: Xamarin.AndroidX.Core.dll => 0xe843daa0 => 30
	i32 3920810846, ; 138: System.IO.Compression.FileSystem.dll => 0xe9b2d35e => 66
	i32 3921031405, ; 139: Xamarin.AndroidX.VersionedParcelable.dll => 0xe9b630ed => 55
	i32 3945713374, ; 140: System.Data.DataSetExtensions.dll => 0xeb2ecede => 64
	i32 3955647286, ; 141: Xamarin.AndroidX.AppCompat.dll => 0xebc66336 => 22
	i32 4105002889, ; 142: Mono.Security.dll => 0xf4ad5f89 => 72
	i32 4151237749, ; 143: System.Core => 0xf76edc75 => 5
	i32 4182413190, ; 144: Xamarin.AndroidX.Lifecycle.ViewModelSavedState.dll => 0xf94a8f86 => 43
	i32 4292120959 ; 145: Xamarin.AndroidX.Lifecycle.ViewModelSavedState => 0xffd4917f => 43
], align 4
@assembly_image_cache_indices = local_unnamed_addr constant [146 x i32] [
	i32 41, i32 59, i32 4, i32 18, i32 50, i32 50, i32 12, i32 28, ; 0..7
	i32 17, i32 51, i32 26, i32 38, i32 70, i32 36, i32 19, i32 8, ; 8..15
	i32 39, i32 57, i32 35, i32 3, i32 6, i32 36, i32 45, i32 17, ; 16..23
	i32 63, i32 66, i32 32, i32 55, i32 23, i32 71, i32 15, i32 65, ; 24..31
	i32 60, i32 47, i32 59, i32 4, i32 39, i32 49, i32 22, i32 42, ; 32..39
	i32 6, i32 53, i32 23, i32 54, i32 34, i32 69, i32 49, i32 29, ; 40..47
	i32 65, i32 21, i32 33, i32 12, i32 68, i32 44, i32 31, i32 7, ; 48..55
	i32 10, i32 58, i32 27, i32 51, i32 5, i32 35, i32 44, i32 58, ; 56..63
	i32 57, i32 24, i32 46, i32 42, i32 40, i32 10, i32 9, i32 37, ; 64..71
	i32 18, i32 7, i32 53, i32 45, i32 46, i32 48, i32 20, i32 61, ; 72..79
	i32 1, i32 64, i32 41, i32 54, i32 31, i32 19, i32 21, i32 16, ; 80..87
	i32 56, i32 29, i32 11, i32 56, i32 52, i32 67, i32 3, i32 9, ; 88..95
	i32 34, i32 38, i32 13, i32 68, i32 62, i32 33, i32 72, i32 27, ; 96..103
	i32 25, i32 16, i32 32, i32 62, i32 24, i32 37, i32 30, i32 1, ; 104..111
	i32 48, i32 69, i32 11, i32 61, i32 15, i32 2, i32 63, i32 25, ; 112..119
	i32 71, i32 67, i32 47, i32 40, i32 2, i32 70, i32 13, i32 26, ; 120..127
	i32 60, i32 20, i32 0, i32 0, i32 28, i32 8, i32 14, i32 14, ; 128..135
	i32 52, i32 30, i32 66, i32 55, i32 64, i32 22, i32 72, i32 5, ; 136..143
	i32 43, i32 43 ; 144..145
], align 4

@marshal_methods_number_of_classes = local_unnamed_addr constant i32 0, align 4

; marshal_methods_class_cache
@marshal_methods_class_cache = global [0 x %struct.MarshalMethodsManagedClass] [
], align 4; end of 'marshal_methods_class_cache' array


@get_function_pointer = internal unnamed_addr global void (i32, i32, i32, i8**)* null, align 4

; Function attributes: "frame-pointer"="none" "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind sspstrong "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn writeonly
define void @xamarin_app_init (void (i32, i32, i32, i8**)* %fn) local_unnamed_addr #0
{
	store void (i32, i32, i32, i8**)* %fn, void (i32, i32, i32, i8**)** @get_function_pointer, align 4
	ret void
}

; Names of classes in which marshal methods reside
@mm_class_names = local_unnamed_addr constant [0 x i8*] zeroinitializer, align 4
@__MarshalMethodName_name.0 = internal constant [1 x i8] c"\00", align 1

; mm_method_names
@mm_method_names = local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	; 0
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		i8* getelementptr inbounds ([1 x i8], [1 x i8]* @__MarshalMethodName_name.0, i32 0, i32 0); name
	}
], align 8; end of 'mm_method_names' array


attributes #0 = { "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind sspstrong "stack-protector-buffer-size"="8" uwtable willreturn writeonly "frame-pointer"="none" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" "stackrealign" }
attributes #1 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nounwind sspstrong "stack-protector-buffer-size"="8" uwtable "frame-pointer"="none" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" "stackrealign" }
attributes #2 = { nounwind }

!llvm.module.flags = !{!0, !1, !2}
!llvm.ident = !{!3}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!2 = !{i32 1, !"NumRegisterParameters", i32 0}
!3 = !{!"Xamarin.Android remotes/origin/d17-5 @ 45b0e144f73b2c8747d8b5ec8cbd3b55beca67f0"}
!llvm.linker.options = !{}
