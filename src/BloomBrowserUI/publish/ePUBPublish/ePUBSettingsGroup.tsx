/** @jsx jsx **/
import { jsx, css } from "@emotion/core";
import React = require("react");
import BookMetadataDialog from "../metadata/BookMetadataDialog";
import { BloomApi } from "../../utils/bloomApi";
import { Typography, FormGroup, Tooltip, Popover } from "@material-ui/core";
import { LocalizedString } from "../../react_components/l10nComponents";
import { ApiCheckbox } from "../../react_components/ApiCheckbox";
import { Link } from "../../react_components/link";
import { SettingsGroup } from "../commonPublish/BasePublishScreen";
import { useL10n } from "../../react_components/l10nHooks";
import { default as InfoIcon } from "@material-ui/icons/InfoOutlined";
import { InfoTooltip } from "../../react_components/icons/InfoTooltip";

export const EPUBSettingsGroup = () => {
    //const [includeImageDescriptionOnPage,setIncludeImageDescriptionOnPage] = BloomApi.useApiBoolean("publish/epub/imageDescriptionSetting", true);
    const [canModifyCurrentBook] = BloomApi.useApiBoolean(
        "common/canModifyCurrentBook",
        false
    );
    const checkoutToEdit = useL10n(
        "Check out the book to use this control",
        "TeamCollection.CheckoutToEdit",
        undefined,
        undefined,
        undefined,
        true
    );

    // controls visibility and placement of the 'tooltip' on the info icon when bookdata is disabled.
    const [anchorEl, setAnchorEl] = React.useState<SVGSVGElement | null>(null);
    const tooltipOpen = Boolean(anchorEl);
    const handlePopoverOpen = (event: React.MouseEvent<SVGSVGElement>) => {
        setAnchorEl(event.currentTarget);
    };
    return (
        <SettingsGroup
            label={useL10n(
                "Accessibility",
                "PublishTab.Epub.Accessibility",
                "Here, the English 'Accessibility' is a common way of referring to technologies that are usable by people with disabilities. With computers, this usually means people with visual impairments. It includes botht he blind and people who might need text to be larger, or who are colorblind, etc."
            )}
        >
            <ApiCheckbox
                english="Include image descriptions on page"
                apiEndpoint="publish/epub/imageDescriptionSetting"
                l10nKey="PublishTab.Epub.IncludeOnPage"
                disabled={false}
            />

            <ApiCheckbox
                english="Use ePUB reader's text size"
                apiEndpoint="publish/epub/removeFontSizesSetting"
                l10nKey="PublishTab.Epub.RemoveFontSizes"
                disabled={false}
                //TODO: priorClickAction={() => this.abortPreview()}
            />
            {/* l10nKey is intentionally not under PublishTab.Epub... we may end up with this link in other places */}
            <Link
                id="a11yCheckerLink"
                l10nKey="AccessibilityCheck.AccessibilityChecker"
                onClick={() =>
                    BloomApi.post("accessibilityCheck/showAccessibilityChecker")
                }
            >
                Accessibility Checker
            </Link>
            <div
                css={css`
                    display: flex;
                    align-items: center;
                `}
            >
                <Link
                    id="bookMetadataDialogLink"
                    l10nKey="PublishTab.BookMetadata"
                    l10nComment="This link opens a dialog box that lets you put in information someone (often a librarian) might use to search for a book with particular characteristics."
                    onClick={() => BookMetadataDialog.show()}
                    disabled={!canModifyCurrentBook}
                >
                    Book Metadata
                </Link>
                {canModifyCurrentBook || (
                    <InfoTooltip
                        css={css`
                            margin-top: 15px; // needed to align, despite supposed centering
                            margin-left: 5px;
                        `}
                        color="gray"
                        size="15px"
                        l10nKey="TeamCollection.CheckoutToEdit"
                        temporarilyDisableI18nWarning={true}
                    >
                        Check out the book to use this control
                    </InfoTooltip>
                )}
            </div>
        </SettingsGroup>
    );
};
