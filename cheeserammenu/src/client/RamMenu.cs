using System.Collections.Generic;
using UnityEngine;
using LogicWorld.UI;
using LICC;
using LogicAPI.Data.BuildingRequests;
using TMPro;
using LogicUI.MenuParts;
using CheeseUtilMod.Client;
using System.IO;
using EccsGuiBuilder.Client.Layouts.Elements;
using EccsGuiBuilder.Client.Layouts.Helper;
using EccsGuiBuilder.Client.Wrappers;
using EccsGuiBuilder.Client.Wrappers.AutoAssign;
using LogicWorld.BuildingManagement;

namespace CheeseRamMenu.Client
{
    public class RamMenu : EditComponentMenu, IAssignMyFields
    {
        public static void init()
        {
            WS.window("CheeseRamMenu")
                .setYPosition(150)
                .configureContent(content => content
                    .layoutVertical()
                    .add(WS.inputField
                        .injectionKey(nameof(filePathInputField))
                        .fixedSize(1000, 80)
                        .setPlaceholderLocalizationKey("CheeseRamMenu.FileFieldHint")
                        .disableRichText()
                    )
                    .add(WS.textLine
                        .setLocalizationKey("CheeseRamMenu.FileNotFound")
                        .injectionKey(nameof(errorText))
                    )
                    .add(WS.button.setLocalizationKey("CheeseRamMenu.FileLoad")
                        .injectionKey(nameof(loadButton))
                        .add<ButtonLayout>()
                    )
                    .addContainer("BottomBox", bottomBox => bottomBox
                        .injectionKey(nameof(bottomSection))
                        .layoutVerticalInnerCentered()
                        .addContainer("BottomInnerBox", innerBox => innerBox
                            .layoutGrowGapVerticalInner() // Normal vertical layouts try to expand more than they have to...
                            .addContainer("BottomBox1", container => container
                                .layoutGrowGapHorizontalInnerCentered()
                                .add(WS.textLine.setLocalizationKey("CheeseRamMenu.AddressLines"))
                                .add(WS.slider
                                    .injectionKey(nameof(addressPegSlider))
                                    .fixedSize(500, 45)
                                    .setInterval(1)
                                    .setMin(4)
                                    .setMax(24)
                                )
                            )
                            .addContainer("BottomBox2", container => container
                                .layoutGrowGapHorizontalInnerCentered()
                                .add(WS.textLine.setLocalizationKey("CheeseRamMenu.BitWidth"))
                                .add(WS.slider
                                    .injectionKey(nameof(widthPegSlider))
                                    .fixedSize(500, 45)
                                    .setInterval(1)
                                    .setMin(1)
                                    .setMax(64)
                                )
                            )
                        )
                    )
                )
                .add<RamMenu>()
                .build();
        }

        [AssignMe]
        public TMP_InputField filePathInputField;
        [AssignMe]
        public HoverButton loadButton;
        [AssignMe]
        public InputSlider addressPegSlider;
        [AssignMe]
        public InputSlider widthPegSlider;
        [AssignMe]
        public GameObject bottomSection;
        [AssignMe]
        public GameObject errorText;

        private bool isComponentResizable;

        protected override void OnStartEditing()
        {
            errorText.SetActive(false);
            if (FirstComponentBeingEdited.ClientCode is RamResizableClient)
            {
                var num_inputs = FirstComponentBeingEdited.Component.Data.InputCount;
                var num_outputs = FirstComponentBeingEdited.Component.Data.OutputCount;
                addressPegSlider.SetValueWithoutNotify(num_inputs - 3 - num_outputs);
                widthPegSlider.SetValueWithoutNotify(num_outputs);
                bottomSection.SetActive(true);
                isComponentResizable = true;
            }
            else
            {
                var num_inputs = FirstComponentBeingEdited.Component.Data.InputCount;
                var num_outputs = FirstComponentBeingEdited.Component.Data.OutputCount;
                addressPegSlider.SetValueWithoutNotify(num_inputs - 3 - num_outputs);
                widthPegSlider.SetValueWithoutNotify(num_outputs);
                bottomSection.SetActive(false);
                isComponentResizable = false;
            }
            filePathInputField.text = "";
            filePathInputField.ActivateInputField();
        }

        public override void Initialize()
        {
            base.Initialize();
            addressPegSlider.OnValueChangedInt += addressCountChanged;
            widthPegSlider.OnValueChangedInt += bitwidthChanged;
            loadButton.OnClickEnd += () =>
            {
                loadFile();
                filePathInputField.ActivateInputField();
            };
            filePathInputField.onSubmit.AddListener(text =>
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    loadFile();
                    filePathInputField.ActivateInputField();
                }
            });
            filePathInputField.onValueChanged.AddListener(_ => errorText.SetActive(false));
        }

        private void bitwidthChanged(int newBitwidth)
        {
            if(!isComponentResizable)
            {
                return;
            }
            BuildRequestManager.SendBuildRequest(new BuildRequest_ChangeDynamicComponentPegCounts(
                FirstComponentBeingEdited.Address,
                newBitwidth + 3 + addressPegSlider.ValueAsInt,
                newBitwidth
            ));
        }

        private void addressCountChanged(int newAddressBitWidth)
        {
            if(!isComponentResizable)
            {
                return;
            }
            BuildRequestManager.SendBuildRequest(new BuildRequest_ChangeDynamicComponentPegCounts(
                FirstComponentBeingEdited.Address,
                newAddressBitWidth + 3 + widthPegSlider.ValueAsInt,
                widthPegSlider.ValueAsInt
            ));
        }

        private void loadFile()
        {
            var loadable = (FileLoadable) FirstComponentBeingEdited.ClientCode;
            var filePath = filePathInputField.text;
            if (File.Exists(filePath))
            {
                var bytes = File.ReadAllBytes(filePath);
                var lineWriter = LConsole.BeginLine();
                loadable.Load(bytes, lineWriter, true);
                lineWriter.End();
            }
            else
            {
                errorText.SetActive(true);
                LConsole.WriteLine($"Unable to load file rich text <mspace=0.65em>'<noparse>{filePath}</noparse>'</mspace> as it does not exist");
            }
        }

        protected override IEnumerable<string> GetTextIDsOfComponentTypesThatCanBeEdited()
        {
            return new string[] {
                "CheeseUtilMod.Ram4aX1b",
                "CheeseUtilMod.Ram8aX1b",
                "CheeseUtilMod.Ram16aX1b",
                "CheeseUtilMod.Ram4aX4b",
                "CheeseUtilMod.Ram8aX4b",
                "CheeseUtilMod.Ram16aX4b",
                "CheeseUtilMod.Ram4aX8b",
                "CheeseUtilMod.Ram8aX8b",
                "CheeseUtilMod.Ram16aX8b",
                "CheeseUtilMod.Ram4aX16b",
                "CheeseUtilMod.Ram8aX16b",
                "CheeseUtilMod.Ram16aX16b",
                "CheeseUtilMod.RamResizable",
            };
        }
    }
}
