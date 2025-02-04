/** @jsx jsx **/
import { jsx, css } from "@emotion/core";

import * as React from "react";
import { useState, useEffect } from "react";
import ToolboxToolReactAdaptor from "../toolboxToolReactAdaptor";
import * as ReactDOM from "react-dom";
import "./overlay.less";
import {
    getEditablePageBundleExports,
    getEditTabBundleExports
} from "../../js/bloomFrames";
import { BubbleManager } from "../../js/bubbleManager";
import { BubbleSpec, TailSpec } from "comicaljs";
import { ToolBottomHelpLink } from "../../../react_components/helpLink";
import FormControl from "@material-ui/core/FormControl";
import Select from "@material-ui/core/Select";
import { Button, MenuItem, Typography } from "@material-ui/core";
import { useL10n } from "../../../react_components/l10nHooks";
import { Div, Span } from "../../../react_components/l10nComponents";
import InputLabel from "@material-ui/core/InputLabel";
import * as toastr from "toastr";
import { default as TrashIcon } from "@material-ui/icons/Delete";
import { BloomApi } from "../../../utils/bloomApi";
import { isLinux } from "../../../utils/isLinux";
import { MuiCheckbox } from "../../../react_components/muiCheckBox";
import { ColorBar } from "./colorBar";
import { ISwatchDefn } from "../../../react_components/colorSwatch";
import {
    defaultBackgroundColors,
    defaultTextColors,
    getSwatchFromBubbleSpecColor,
    getSpecialColorName
} from "./overlayToolColorHelper";
import { IColorPickerDialogProps } from "../../../react_components/colorPickerDialog";
import * as tinycolor from "tinycolor2";
import { showSignLanguageTool } from "../../js/bloomVideo";
import { kBloomBlue } from "../../../bloomMaterialUITheme";
import { RequiresBloomEnterpriseOverlayWrapper } from "../../../react_components/requiresBloomEnterprise";

const OverlayToolControls: React.FunctionComponent = () => {
    const l10nPrefix = "ColorPicker.";
    type BubbleType = "text" | "image" | "video" | undefined;

    // Declare all the hooks
    const [style, setStyle] = useState("none");
    const [outlineColor, setOutlineColor] = useState<string | undefined>(
        undefined
    );
    const [bubbleType, setBubbleType] = useState<BubbleType>(undefined);
    const [showTailChecked, setShowTailChecked] = useState(false);
    const [isRoundedCornersChecked, setIsRoundedCornersChecked] = useState(
        false
    );
    const [isXmatter, setIsXmatter] = useState(true);
    // If the book is locked, we don't want users dragging things onto the page.
    const [isBookLocked, setIsBookLocked] = useState(false);
    // This 'counter' increments on new page ready so we can re-check if the book is locked.
    const [pageRefreshIndicator, setPageRefreshIndicator] = useState(0);

    // Calls to useL10n
    const deleteTooltip = useL10n("Delete", "Common.Delete");
    const duplicateTooltip = useL10n(
        "Duplicate",
        "EditTab.Toolbox.ComicTool.Options.Duplicate"
    );

    // While renaming Comic -> Overlay, I (gjm) intentionally left several (21) "keys" with
    // the old "ComicTool" to avoid the whole deprecate/invalidate/retranslate issue.

    // Setup for color picker, in case we need it.
    const textColorTitle = useL10n(
        "Text Color",
        "EditTab.Toolbox.ComicTool.Options.TextColor"
    );
    const backgroundColorTitle = useL10n(
        "Background Color",
        "EditTab.Toolbox.ComicTool.Options.BackgroundColor"
    );

    // Text color swatch
    // defaults to "black" text color
    const [textColorSwatch, setTextColorSwatch] = useState(
        defaultTextColors[0]
    );

    // Background color swatch
    // defaults to "white" background color
    const [backgroundColorSwatch, setBackgroundColorSwatch] = useState(
        defaultBackgroundColors[1]
    );

    // If bubbleType is not undefined, corresponds to the active bubble's family.
    // Otherwise, corresponds to the most recently active bubble's family.
    const [currentFamilySpec, setCurrentFamilySpec] = useState<
        BubbleSpec | undefined
    >(undefined);

    // Callback to initialize bubbleEditing and get the initial bubbleSpec
    const bubbleSpecInitialization = () => {
        const bubbleManager = OverlayTool.bubbleManager();
        if (!bubbleManager) {
            console.assert(
                false,
                "ERROR: Bubble manager is not initialized yet. Please investigate!"
            );
            return;
        }

        bubbleManager.turnOnBubbleEditing();
        bubbleManager.turnOnHidingImageButtons();
        bubbleManager.deselectVideoContainers();

        const bubbleSpec = bubbleManager.getSelectedFamilySpec();

        // The callback function is (currently) called when switching between bubbles, but is not called
        // if the tail spec changes, or for style and similar changes to the bubble that are initiated by React.
        bubbleManager.requestBubbleChangeNotification(
            (bubble: BubbleSpec | undefined) => {
                setCurrentFamilySpec(bubble);
            }
        );

        setCurrentFamilySpec(bubbleSpec);
    };

    // Enhance: if we don't want to have a static, or don't want
    // this function to know about OverlayTool, we could just pass
    // a setter for this as a property.
    OverlayTool.theOneOverlayTool!.callOnNewPageReady = () => {
        bubbleSpecInitialization();
        setIsXmatter(ToolboxToolReactAdaptor.isXmatter());
        const count = pageRefreshIndicator;
        setPageRefreshIndicator(count + 1);
    };

    // Reset UI when current bubble spec changes (e.g. user clicked on a bubble).
    useEffect(() => {
        if (currentFamilySpec) {
            setStyle(currentFamilySpec.style);
            setShowTailChecked(
                currentFamilySpec.tails && currentFamilySpec.tails.length > 0
            );
            setIsRoundedCornersChecked(
                !!currentFamilySpec.cornerRadiusX &&
                    !!currentFamilySpec.cornerRadiusY &&
                    currentFamilySpec.cornerRadiusX > 0 &&
                    currentFamilySpec.cornerRadiusY > 0
            );
            setOutlineColor(currentFamilySpec.outerBorderColor);
            const backColor = getBackgroundColorValue(currentFamilySpec);
            const newSwatch = getSwatchFromBubbleSpecColor(backColor);
            setBackgroundColorSwatch(newSwatch);

            const bubbleMgr = OverlayTool.bubbleManager();
            setBubbleType(getBubbleType(bubbleMgr));
            if (bubbleMgr) {
                // Get the current bubble's textColor and set it
                const bubbleTextColor = bubbleMgr.getTextColor();
                const newSwatch = getSwatchFromBubbleSpecColor(bubbleTextColor);
                setTextColorSwatch(newSwatch);
            }
        } else {
            setBubbleType(undefined);
        }
    }, [currentFamilySpec]);

    const getBubbleType = (mgr: BubbleManager | undefined): BubbleType => {
        if (!mgr) {
            return undefined;
        }
        if (mgr.isActiveElementPictureOverPicture()) {
            return "image";
        }
        return mgr.isActiveElementVideoOverPicture() ? "video" : "text";
    };

    useEffect(() => {
        // Get the lock/unlock state from C#-land. We have to do this every time the page is refreshed
        // (see 'callOnNewPageReady') in case the user temporarily unlocks the book.
        BloomApi.get("edit/pageControls/requestState", result => {
            const jsonObj = result.data;
            setIsBookLocked(jsonObj.BookLockedState === "BookLocked");
        });
    }, [pageRefreshIndicator]);

    // Callback for style changed
    const handleStyleChanged = event => {
        const newStyle = event.target.value;

        // Update the toolbox controls
        setStyle(newStyle);

        // Update the Comical canvas on the page frame
        const bubbleMgr = OverlayTool.bubbleManager();
        if (bubbleMgr) {
            const newBubbleProps = {
                style: newStyle
            };

            // BL-8537: If we are choosing "caption" style, we make sure that the background color is opaque.
            let backgroundColorArray = currentFamilySpec?.backgroundColors;
            if (
                newStyle === "caption" &&
                backgroundColorArray &&
                backgroundColorArray.length === 1
            ) {
                backgroundColorArray[0] = setOpaque(backgroundColorArray[0]);
            }

            // Avoid setting backgroundColorArray if it's just undefined.
            // Setting it to be undefined defines it as a property. This means that when objects are merged,
            // this object is considered to have a backgroundColors property, even though it may not be visible via JSON.stringify and even though
            // you may not have intended for it to overwrite prior values.
            if (backgroundColorArray !== undefined) {
                newBubbleProps["backgroundColors"] = backgroundColorArray;
            }

            const newSpec = bubbleMgr.updateSelectedFamilyBubbleSpec(
                newBubbleProps
            );
            // We do this because the new style's spec may affect Show Tail, or background opacity too.
            setCurrentFamilySpec(newSpec);
        }
    };

    // Callback for show tail checkbox changed
    // Presently, only disabled if style is "none".
    const handleShowTailChanged = (value: boolean) => {
        setShowTailChecked(value);

        // Update the Comical canvas on the page frame
        const bubbleMgr = OverlayTool.bubbleManager();
        if (bubbleMgr) {
            bubbleMgr.updateSelectedFamilyBubbleSpec({
                tails: value ? [bubbleMgr.getDefaultTailSpec() as TailSpec] : []
            });
        }
    };

    // Callback for rounded corners checkbox changed
    const handleRoundedCornersChanged = (newValue: boolean | undefined) => {
        setIsRoundedCornersChecked(newValue || false);

        // Update the Comical canvas on the page frame
        const bubbleMgr = OverlayTool.bubbleManager();
        if (bubbleMgr) {
            const radius = newValue ? 8 : undefined; // 8 is semi-arbitrary for now. We may add a control in the future to set it.
            bubbleMgr.updateSelectedFamilyBubbleSpec({
                cornerRadiusX: radius,
                cornerRadiusY: radius
            });
        }
    };

    const getBackgroundColorValue = (familySpec: BubbleSpec): string => {
        const bubbleMgr = OverlayTool.bubbleManager();
        if (bubbleMgr) {
            const backgroundColorArray = bubbleMgr.getBackgroundColorArray(
                familySpec
            );
            if (backgroundColorArray.length === 1) {
                return backgroundColorArray[0]; // This could be a hex string or an rgba() string
            }
            const specialName = getSpecialColorName(backgroundColorArray);
            return specialName ? specialName : "white"; // maybe from a later version of Bloom? All we can do.
        } else {
            return "white";
        }
    };

    // We come into this from chooser change
    const updateTextColor = (newColorSwatch: ISwatchDefn) => {
        const color = newColorSwatch.colors[0]; // text color is always monochrome
        const bubbleMgr = OverlayTool.bubbleManager();
        if (bubbleMgr) {
            // Update the toolbox controls
            setTextColorSwatch(newColorSwatch);

            bubbleMgr.setTextColor(color);
            // BL-9936/11104 Without this, bubble manager is up-to-date, but React doesn't know about it.
            updateReactFromComical(bubbleMgr);
        }
    };

    const noteInputFocused = (input: HTMLElement) =>
        OverlayTool.bubbleManager()?.setThingToFocusAfterSettingColor(input);

    // We come into this from chooser change
    const updateBackgroundColor = (newColorSwatch: ISwatchDefn) => {
        const bubbleMgr = OverlayTool.bubbleManager();
        if (bubbleMgr) {
            // Update the toolbox controls
            setBackgroundColorSwatch(newColorSwatch);

            // Update the Comical canvas on the page frame
            const backgroundColors = newColorSwatch.colors;
            bubbleMgr.setBackgroundColor(
                backgroundColors,
                newColorSwatch.opacity
            );
            // BL-9936/11104 Without this, bubble manager is up-to-date, but React doesn't know about it.
            updateReactFromComical(bubbleMgr);
        }
    };

    // We use this to get React's 'currentFamilySpec' up-to-date with what comical has, since some minor
    // React-initiated changes don't trigger BubbleManager's 'requestBubbleChangeNotification'.
    // Changing 'currentFamilySpec' is what updates the UI of the toolbox in general.
    const updateReactFromComical = (bubbleMgr: BubbleManager) => {
        const newSpec = bubbleMgr.getSelectedFamilySpec();
        setCurrentFamilySpec(newSpec);
    };

    // Callback when outline color of the bubble is changed
    const handleOutlineColorChanged = event => {
        let newValue = event.target.value;

        if (newValue === "none") {
            newValue = undefined;
        }

        const bubbleMgr = OverlayTool.bubbleManager();
        if (bubbleMgr) {
            // Update the toolbox controls
            setOutlineColor(newValue);

            // Update the Comical canvas on the page frame
            bubbleMgr.updateSelectedFamilyBubbleSpec({
                outerBorderColor: newValue
            });
        }
    };

    const handleChildBubbleLinkClick = event => {
        const bubbleManager = OverlayTool.bubbleManager();

        if (bubbleManager) {
            const parentElement = bubbleManager.getActiveElement();

            if (!parentElement) {
                // No parent to attach to
                toastr.info("No element is currently active.");
                return;
            }

            // Enhance: Is there a cleaner way to keep activeBubbleSpec up to date?
            // Comical would need to call the notifier a lot more often like when the tail moves.

            // Retrieve the latest bubbleSpec
            const bubbleSpec = bubbleManager.getSelectedItemBubbleSpec();
            const [
                offsetX,
                offsetY
            ] = OverlayTool.GetChildPositionFromParentBubble(
                parentElement,
                bubbleSpec
            );
            bubbleManager.addChildOverPictureElementAndRefreshPage(
                parentElement,
                offsetX,
                offsetY
            );
        }
    };

    const ondragstart = (ev: React.DragEvent<HTMLElement>, style: string) => {
        // Here "bloomBubble" is a unique, private data type recognised
        // by ondragover and ondragdrop methods that BubbleManager
        // attaches to bloom image containers. It doesn't make sense to
        // drag these objects anywhere else, so they don't need any of
        // the common data types.
        ev.dataTransfer.setData("bloomBubble", style);
    };

    const ondragend = (ev: React.DragEvent<HTMLElement>, style: string) => {
        const bubbleManager = OverlayTool.bubbleManager();
        // The Linux/Mono/Geckofx environment does not produce the dragenter, dragover,
        // and drop events for the targeted element.  It does produce the dragend event
        // for the source element with screen coordinates of where the mouse was released.
        // This can be used to simulate the drop event with coordinate transformation.
        // See https://issues.bloomlibrary.org/youtrack/issue/BL-7958.
        if (isLinux() && bubbleManager && !isBookLocked) {
            bubbleManager.addOverPictureElementWithScreenCoords(
                ev.screenX,
                ev.screenY,
                style
            );
        }
    };

    const deleteBubble = () => {
        const bubbleManager = OverlayTool.bubbleManager();
        if (bubbleManager) {
            const active = bubbleManager.getActiveElement();
            if (active) {
                bubbleManager.deleteTOPBox(active);
            }
        }
    };

    const duplicateBubble = () => {
        const bubbleManager = OverlayTool.bubbleManager();
        if (bubbleManager) {
            const active = bubbleManager.getActiveElement();
            if (active) {
                bubbleManager.duplicateTOPBox(active);
            }
        }
    };

    const styleSupportsRoundedCorners = (
        currentBubbleSpec: BubbleSpec | undefined
    ) => {
        if (!currentBubbleSpec) {
            return false;
        }

        const bgColors = currentBubbleSpec.backgroundColors;
        if (bgColors && bgColors.includes("transparent")) {
            // Don't allow on transparent bubbles
            return false;
        }

        switch (currentBubbleSpec.style) {
            case "caption":
                return true;
            case "none":
                // Just text - rounded corners applicable if it has a background color
                return bgColors && bgColors.length > 0;
            default:
                return false;
        }
    };

    const launchTextColorChooser = () => {
        const colorPickerDialogProps: IColorPickerDialogProps = {
            noAlphaSlider: true,
            noGradientSwatches: true,
            localizedTitle: textColorTitle,
            initialColor: textColorSwatch,
            defaultSwatchColors: defaultTextColors,
            onChange: color => updateTextColor(color),
            onInputFocus: noteInputFocused
        };
        getEditTabBundleExports().showColorPickerDialog(colorPickerDialogProps);
    };

    // The background color chooser uses an alpha slider for transparency.
    // Unfortunately, with an alpha slider, the hex input will automatically switch to rgb
    // the moment the user sets alpha to anything but max opacity.
    const launchBackgroundColorChooser = (noAlpha: boolean) => {
        const colorPickerDialogProps: IColorPickerDialogProps = {
            noAlphaSlider: noAlpha,
            localizedTitle: backgroundColorTitle,
            initialColor: backgroundColorSwatch,
            defaultSwatchColors: defaultBackgroundColors,
            onChange: color => updateBackgroundColor(color),
            onInputFocus: noteInputFocused
        };
        // If the background color is fully transparent, change it to fully opaque
        // so that the user can choose a color immediately (and adjust opacity to
        // a lower value as well if wanted).
        // See https://issues.bloomlibrary.org/youtrack/issue/BL-9922.
        if (colorPickerDialogProps.initialColor.opacity === 0)
            colorPickerDialogProps.initialColor.opacity = 100;
        getEditTabBundleExports().showColorPickerDialog(colorPickerDialogProps);
    };

    const needToCalculateTransparency = (): boolean => {
        const opacityDecimal = backgroundColorSwatch.opacity;
        return opacityDecimal < 1.0;
    };

    const percentTransparentFromOpacity = !needToCalculateTransparency()
        ? "0" // We shouldn't call this under these circumstances.
        : (100 - (backgroundColorSwatch.opacity as number) * 100).toFixed(0);

    const transparencyString = useL10n(
        "Percent Transparent",
        l10nPrefix + "PercentTransparent",
        "",
        percentTransparentFromOpacity
    );

    // We need to calculate this (even though we may not need to display it) to keep from violating
    // React's rule about not changing the number of hooks rendered.
    // This is even more important now that we don't show this part of the UI sometimes (BL-9976)!
    const percentTransparencyString =
        percentTransparentFromOpacity === "0" ? undefined : transparencyString;

    // Note: Make sure bubble spec is the current ITEM's spec, not the current FAMILY's spec.
    const isChild = (bubbleSpec: BubbleSpec | undefined) => {
        const order = bubbleSpec?.order ?? 0;
        return order > 1;
    };

    const bubbleManager = OverlayTool.bubbleManager();
    const currentItemSpec = bubbleManager?.getSelectedItemBubbleSpec();

    // BL-8537 Because of the black shadow background, partly transparent backgrounds don't work for
    // captions. We'll use this to tell the color chooser not to show the alpha option.
    const isCaption = currentFamilySpec?.style === "caption";

    const getControlOptionsRegion = (): JSX.Element => {
        switch (bubbleType) {
            case "image":
                return (
                    <div id="videoOrImageSubstituteSection">
                        <Typography
                            css={css`
                                // "!important" is needed to keep .MuiTypography-root from overriding
                                margin: 15px 15px 0 15px !important;
                                text-align: center;
                            `}
                        >
                            <Span l10nKey="EditTab.Toolbox.ComicTool.Options.ImageSelected">
                                There are no options for this kind of overlay
                            </Span>
                        </Typography>
                    </div>
                );
            case "video":
                return (
                    <div id="videoOrImageSubstituteSection">
                        <Button
                            css={css`
                                // Had to add "!important"s because .MuiButton-contained overrode them!
                                background-color: ${kBloomBlue} !important;
                                text-align: center;
                                margin: 20px 10px 5px 10px !important;
                                padding: 5px 0 !important; // defeat huge 'contained' style padding-right
                            `}
                            onClick={showSignLanguageTool}
                            size="large"
                            variant="contained"
                        >
                            <Typography
                                css={css`
                                    color: white;
                                `}
                            >
                                <Span l10nKey="EditTab.Toolbox.ComicTool.Options.ShowSignLanguageTool">
                                    Show Sign Language Tool
                                </Span>
                            </Typography>
                        </Button>
                    </div>
                );
            case undefined:
            case "text":
                return (
                    <form autoComplete="off">
                        <FormControl>
                            <InputLabel htmlFor="bubble-style-dropdown">
                                <Span l10nKey="EditTab.Toolbox.ComicTool.Options.Style">
                                    Style
                                </Span>
                            </InputLabel>
                            <Select
                                value={style}
                                onChange={event => {
                                    handleStyleChanged(event);
                                }}
                                className="bubbleOptionDropdown"
                                inputProps={{
                                    name: "style",
                                    id: "bubble-style-dropdown"
                                }}
                                MenuProps={{
                                    className: "bubble-options-dropdown-menu"
                                }}
                            >
                                <MenuItem value="caption">
                                    <Div l10nKey="EditTab.Toolbox.ComicTool.Options.Style.Caption">
                                        Caption
                                    </Div>
                                </MenuItem>
                                <MenuItem value="pointedArcs">
                                    <Div l10nKey="EditTab.Toolbox.ComicTool.Options.Style.Exclamation">
                                        Exclamation
                                    </Div>
                                </MenuItem>
                                <MenuItem value="none">
                                    <Div l10nKey="EditTab.Toolbox.ComicTool.Options.Style.JustText">
                                        Just Text
                                    </Div>
                                </MenuItem>
                                <MenuItem value="speech">
                                    <Div l10nKey="EditTab.Toolbox.ComicTool.Options.Style.Speech">
                                        Speech
                                    </Div>
                                </MenuItem>
                                <MenuItem value="ellipse">
                                    <Div l10nKey="EditTab.Toolbox.ComicTool.Options.Style.Ellipse">
                                        Ellipse
                                    </Div>
                                </MenuItem>
                                <MenuItem value="thought">
                                    <Div l10nKey="EditTab.Toolbox.ComicTool.Options.Style.Thought">
                                        Thought
                                    </Div>
                                </MenuItem>
                                <MenuItem value="circle">
                                    <Div l10nKey="EditTab.Toolbox.ComicTool.Options.Style.Circle">
                                        Circle
                                    </Div>
                                </MenuItem>
                            </Select>
                            <div className="comicCheckbox">
                                <MuiCheckbox
                                    label="Show Tail"
                                    l10nKey="EditTab.Toolbox.ComicTool.Options.ShowTail"
                                    checked={showTailChecked}
                                    disabled={isChild(currentItemSpec)}
                                    onCheckChanged={v => {
                                        handleShowTailChanged(v as boolean);
                                    }}
                                    deprecatedVersionWhichDoesntEnsureMultilineLabelsWork={
                                        true
                                    }
                                />
                            </div>
                            <div className="comicCheckbox">
                                <MuiCheckbox
                                    label="Rounded Corners"
                                    l10nKey="EditTab.Toolbox.ComicTool.Options.RoundedCorners"
                                    checked={isRoundedCornersChecked}
                                    disabled={
                                        !styleSupportsRoundedCorners(
                                            currentFamilySpec
                                        )
                                    }
                                    onCheckChanged={newValue => {
                                        handleRoundedCornersChanged(newValue);
                                    }}
                                    deprecatedVersionWhichDoesntEnsureMultilineLabelsWork={
                                        true
                                    }
                                />
                            </div>
                        </FormControl>
                        <FormControl>
                            <InputLabel htmlFor="text-color-bar" shrink={true}>
                                <Span l10nKey="EditTab.Toolbox.ComicTool.Options.TextColor">
                                    Text Color
                                </Span>
                            </InputLabel>
                            <ColorBar
                                id="text-color-bar"
                                onClick={launchTextColorChooser}
                                swatch={textColorSwatch}
                            />
                        </FormControl>
                        <FormControl>
                            <InputLabel
                                shrink={true}
                                htmlFor="background-color-bar"
                            >
                                <Span l10nKey="EditTab.Toolbox.ComicTool.Options.BackgroundColor">
                                    Background Color
                                </Span>
                            </InputLabel>
                            <ColorBar
                                id="background-color-bar"
                                onClick={() =>
                                    launchBackgroundColorChooser(isCaption)
                                }
                                swatch={backgroundColorSwatch}
                                text={percentTransparencyString}
                            />
                        </FormControl>
                        <FormControl>
                            <InputLabel htmlFor="bubble-outlineColor-dropdown">
                                <Span l10nKey="EditTab.Toolbox.ComicTool.Options.OuterOutlineColor">
                                    Outer Outline Color
                                </Span>
                            </InputLabel>
                            <Select
                                value={outlineColor ? outlineColor : "none"}
                                className="bubbleOptionDropdown"
                                inputProps={{
                                    name: "outlineColor",
                                    id: "bubble-outlineColor-dropdown"
                                }}
                                MenuProps={{
                                    className: "bubble-options-dropdown-menu"
                                }}
                                onChange={event => {
                                    handleOutlineColorChanged(event);
                                }}
                            >
                                <MenuItem value="none">
                                    <Div l10nKey="EditTab.Toolbox.ComicTool.Options.OuterOutlineColor.None">
                                        None
                                    </Div>
                                </MenuItem>
                                <MenuItem value="yellow">
                                    <Div l10nKey="Common.Colors.Yellow">
                                        Yellow
                                    </Div>
                                </MenuItem>
                                <MenuItem value="crimson">
                                    <Div l10nKey="Common.Colors.Crimson">
                                        Crimson
                                    </Div>
                                </MenuItem>
                            </Select>
                        </FormControl>
                        <Button
                            onClick={event => handleChildBubbleLinkClick(event)}
                        >
                            <Div l10nKey="EditTab.Toolbox.ComicTool.Options.AddChildBubble">
                                Add Child Bubble
                            </Div>
                        </Button>
                    </form>
                );
        }
    };

    return (
        <div id="overlayToolControls">
            <RequiresBloomEnterpriseOverlayWrapper>
                <div
                    id={"overlayToolControlShapeChooserRegion"}
                    className={!isXmatter ? "" : "disabled"}
                >
                    <Div
                        l10nKey="EditTab.Toolbox.ComicTool.DragInstructions"
                        className="overlayToolControlDragInstructions"
                    >
                        Drag any of these overlays onto the image:
                    </Div>
                    <div className={"shapeChooserRow"} id={"shapeChooserRow1"}>
                        <img
                            id="shapeChooserSpeechBubble"
                            className="overlayToolControlDraggableBubble"
                            src="comic-icon.svg"
                            draggable={!isBookLocked} // insufficient to prevent dragging!
                            onDragStart={
                                !isBookLocked
                                    ? ev => ondragstart(ev, "speech")
                                    : undefined
                            }
                            onDragEnd={
                                !isBookLocked
                                    ? ev => ondragend(ev, "speech")
                                    : undefined
                            }
                        />
                        <img
                            id="shapeChooserImagePlaceholder"
                            className="comicToolControlDraggableBubble"
                            src="image-overlay.svg"
                            draggable={!isBookLocked}
                            onDragStart={
                                !isBookLocked
                                    ? ev => ondragstart(ev, "image")
                                    : undefined
                            }
                            onDragEnd={
                                !isBookLocked
                                    ? ev => ondragend(ev, "image")
                                    : undefined
                            }
                        />
                        <img
                            id="shapeChooserVideoPlaceholder"
                            className="comicToolControlDraggableBubble"
                            src="sign-language-overlay.svg"
                            draggable={!isBookLocked}
                            onDragStart={
                                !isBookLocked
                                    ? ev => ondragstart(ev, "video")
                                    : undefined
                            }
                            onDragEnd={
                                !isBookLocked
                                    ? ev => ondragend(ev, "video")
                                    : undefined
                            }
                        />
                    </div>
                    <div className={"shapeChooserRow"} id={"shapeChooserRow2"}>
                        <Span
                            id="shapeChooserTextBlock"
                            l10nKey="EditTab.Toolbox.ComicTool.TextBlock"
                            className="overlayToolControlDraggableBubble"
                            draggable={!isBookLocked}
                            onDragStart={
                                !isBookLocked
                                    ? ev => ondragstart(ev, "none")
                                    : undefined
                            }
                            onDragEnd={
                                !isBookLocked
                                    ? ev => ondragend(ev, "none")
                                    : undefined
                            }
                        >
                            Text Block
                        </Span>
                        <Span
                            id="shapeChooserCaption"
                            l10nKey="EditTab.Toolbox.ComicTool.Options.Style.Caption"
                            className="overlayToolControlDraggableBubble"
                            draggable={!isBookLocked}
                            onDragStart={
                                !isBookLocked
                                    ? ev => ondragstart(ev, "caption")
                                    : undefined
                            }
                            onDragEnd={
                                !isBookLocked
                                    ? ev => ondragend(ev, "caption")
                                    : undefined
                            }
                        >
                            Caption
                        </Span>
                    </div>
                </div>
                <div
                    id={"overlayToolControlOptionsRegion"}
                    className={bubbleType && !isXmatter ? "" : "disabled"}
                >
                    {getControlOptionsRegion()}
                    <div className="option-button-row">
                        <div title={deleteTooltip}>
                            <TrashIcon
                                id="trashIcon"
                                color="primary"
                                onClick={() => deleteBubble()}
                            />
                        </div>
                        <div title={duplicateTooltip}>
                            <img
                                className="duplicate-bubble-icon"
                                src="duplicate-bubble.svg"
                                onClick={() => duplicateBubble()}
                            />
                        </div>
                    </div>
                </div>
                <div id="overlayToolControlFillerRegion" />
                <div id={"overlayToolControlFooterRegion"}>
                    <ToolBottomHelpLink helpId="Tasks/Edit_tasks/Overlay_Tool/Overlay_Tool_overview.htm" />
                </div>
            </RequiresBloomEnterpriseOverlayWrapper>
        </div>
    );
};
export default OverlayToolControls;

export class OverlayTool extends ToolboxToolReactAdaptor {
    public static theOneOverlayTool: OverlayTool | undefined;

    public callOnNewPageReady: () => void | undefined;

    public constructor() {
        super();

        OverlayTool.theOneOverlayTool = this;
    }

    public makeRootElement(): HTMLDivElement {
        const root = document.createElement("div");
        root.setAttribute("class", "OverlayBody");

        ReactDOM.render(<OverlayToolControls />, root);
        return root as HTMLDivElement;
    }

    public id(): string {
        return "overlay";
    }

    public isExperimental(): boolean {
        return false;
    }

    public toolRequiresEnterprise(): boolean {
        return true;
    }

    public beginRestoreSettings(settings: string): JQueryPromise<void> {
        // Nothing to do, so return an already-resolved promise.
        const result = $.Deferred<void>();
        result.resolve();
        return result;
    }

    public newPageReady() {
        const bubbleManager = OverlayTool.bubbleManager();
        if (!bubbleManager) {
            // probably the toolbox just finished loading before the page.
            // No clean way to fix this
            window.setTimeout(() => this.newPageReady(), 100);
            return;
        }

        if (this.callOnNewPageReady) {
            this.callOnNewPageReady();
        } else {
            console.assert(
                false,
                "CallOnNewPageReady is always expected to be defined but it is not."
            );
        }
    }

    public detachFromPage() {
        const bubbleManager = OverlayTool.bubbleManager();
        if (bubbleManager) {
            // For now we are leaving bubble editing on, because even with the toolbox hidden,
            // the user might edit text, delete bubbles, move handles, etc.
            // We turn it off only when about to save the page.
            //bubbleManager.turnOffBubbleEditing();

            bubbleManager.turnOffHidingImageButtons();
            bubbleManager.detachBubbleChangeNotification();
        }
    }

    public static bubbleManager(): BubbleManager | undefined {
        const exports = getEditablePageBundleExports();
        return exports ? exports.getTheOneBubbleManager() : undefined;
    }

    // Returns a 2-tuple containing the desired x and y offsets of the child bubble from the parent bubble
    //   (i.e., offsetX = child.left - parent.left)
    public static GetChildPositionFromParentBubble(
        parentElement: HTMLElement,
        parentBubbleSpec: BubbleSpec | undefined
    ): number[] {
        let offsetX = parentElement.clientWidth;
        let offsetY = parentElement.clientHeight;

        if (
            parentBubbleSpec &&
            parentBubbleSpec.tails &&
            parentBubbleSpec.tails.length > 0
        ) {
            const tail = parentBubbleSpec.tails[0];

            const bubbleCenterX =
                parentElement.offsetLeft + parentElement.clientWidth / 2.0;
            const bubbleCenterY =
                parentElement.offsetTop + parentElement.clientHeight / 2.0;

            const deltaX = tail.tipX - bubbleCenterX;
            const deltaY = tail.tipY - bubbleCenterY;

            // Place the new child in the opposite quandrant of the tail
            if (deltaX > 0) {
                // ENHANCE: SHould be the child's width
                offsetX = -parentElement.clientWidth;
            } else {
                offsetX = parentElement.clientWidth;
            }

            if (deltaY > 0) {
                // ENHANCE: SHould be the child's height
                offsetY = -parentElement.clientHeight;
            } else {
                offsetY = parentElement.clientHeight;
            }
        }

        return [offsetX, offsetY];
    }
}

function setOpaque(color: string) {
    let firstColor = new tinycolor(color);
    firstColor.setAlpha(1.0);
    return firstColor.toHexString();
}
