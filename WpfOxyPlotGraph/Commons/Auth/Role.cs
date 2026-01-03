using System;

namespace WpfOxyPlotGraph.Commons.Auth
{
	[Flags]
	public enum Role
	{
		None = 0,
		Admin = 1 << 0,     // 관리자
		Doctor = 1 << 1,    // 의사
		Nurse = 1 << 2,     // 간호
		FrontDesk = 1 << 3, // 원무
		Auditor = 1 << 4    // 감사
	}
}


