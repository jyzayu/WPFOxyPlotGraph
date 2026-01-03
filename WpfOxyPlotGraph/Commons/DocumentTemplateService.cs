using System;
using System.Collections.Generic;
using System.Text;
using WpfOxyPlotGraph.Models;

namespace WpfOxyPlotGraph.Commons
{
	public enum DocumentTemplateType
	{
		EncounterSummary,
		Prescription,
		LabOrder
	}

	public class DocumentTemplateService
	{
		// Simple in-memory defaults; could be loaded from configuration later
		private readonly Dictionary<DocumentTemplateType, string> _defaults = new()
		{
			{ DocumentTemplateType.EncounterSummary,
				"진료 요약\r\n\r\n환자: {PatientName}\r\n생년월일(식별): {PatientId}\r\n내원일시: {VisitAt}\r\n\r\n주요 진단: {Diagnosis}\r\n메모: {Notes}\r\n" },
			{ DocumentTemplateType.Prescription,
				"처방전\r\n\r\n환자: {PatientName}\r\n식별: {PatientId}\r\n진료일: {VisitAt}\r\n\r\n처방 내용:\r\n- (여기에 약품/용량/기간 등을 기입)\r\n\r\n의사 서명: __________\r\n" },
			{ DocumentTemplateType.LabOrder,
				"검사의뢰서\r\n\r\n환자: {PatientName}\r\n식별: {PatientId}\r\n진료일: {VisitAt}\r\n\r\n의뢰 검사:\r\n- (여기에 검사 항목을 기입)\r\n\r\n특이사항: {Notes}\r\n" }
		};

		public string ApplyEncounterTemplate(DocumentTemplateType type, Patient patient, Encounter encounter)
		{
			string tpl = _defaults.TryGetValue(type, out var v) ? v : string.Empty;
			if (string.IsNullOrEmpty(tpl)) return string.Empty;
			return tpl
				.Replace("{PatientName}", patient?.Name ?? string.Empty)
				.Replace("{PatientId}", patient?.Id.ToString() ?? string.Empty)
				.Replace("{VisitAt}", encounter?.VisitAt.ToString("yyyy-MM-dd HH:mm") ?? string.Empty)
				.Replace("{Diagnosis}", encounter?.Diagnosis ?? string.Empty)
				.Replace("{Notes}", encounter?.Notes ?? string.Empty);
		}
	}
}


